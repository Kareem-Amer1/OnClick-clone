using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Core.Entites;
using Talabat.Core.Services;

namespace Talabat.APlS.Controllers
{
    public class DeliveryController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGeocodingService _geocodingService;
        private readonly ITspOptimizerService _tspOptimizer;
        private readonly IDeliveryCostEstimator _costEstimator;
        private readonly IRouteTimeCalculator _routeTimeCalculator;

        public DeliveryController(
            IOrderService orderService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IGeocodingService geocodingService,
            ITspOptimizerService tspOptimizer,
            IDeliveryCostEstimator costEstimator,
            IRouteTimeCalculator routeTimeCalculator)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _geocodingService = geocodingService;
            _tspOptimizer = tspOptimizer;
            _costEstimator = costEstimator;
            _routeTimeCalculator = routeTimeCalculator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<bool>> Login([FromBody] DeliveryAuthDto authDto)
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
            var deliveryPerson = deliveryMethods.FirstOrDefault(d => 
                d.Email == authDto.Email && 
                d.Password == authDto.Password);

            if (deliveryPerson == null)
                return Unauthorized(new ApiResponse(401, "Invalid delivery credentials"));

            return Ok(new { 
                success = true,
                deliveryPerson = new {
                    id = deliveryPerson.Id,
                    email = deliveryPerson.Email,
                    shortName = deliveryPerson.ShortName,
                    street = deliveryPerson.Street,
                    city = deliveryPerson.City,
                    country = deliveryPerson.Country,
                    address = string.IsNullOrEmpty(deliveryPerson.Street) ? null : $"{deliveryPerson.Street}, {deliveryPerson.City}, {deliveryPerson.Country}",
                    phoneNumber = deliveryPerson.PhoneNumber,
                    deliveryTime = deliveryPerson.DeliveryTime,
                    description = deliveryPerson.Description,
                    cost = deliveryPerson.Cost,
                    statusOfDelivery = deliveryPerson.StatusOfDelivery,
                    startShift = FormatTimeSpan(deliveryPerson.StartShift),
                    endShift = FormatTimeSpan(deliveryPerson.EndShift)
                }
            });
        }
        
        [HttpGet("orders/{deliveryPersonId}")]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForDeliveryPerson(int deliveryPersonId)
        {
            var orders = await _orderService.GetOrdersForDeliveryPersonAsync(deliveryPersonId);
            
            if (orders == null || !orders.Any())
                return Ok(new List<OrderToReturnDto>());
                
            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }
        
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
                return NotFound(new ApiResponse(404, "Order not found"));
                
            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }
        
        [HttpPut("order/{orderId}/track")]
        public async Task<ActionResult<OrderToReturnDto>> UpdateOrderTrackStatus(int orderId, [FromBody] UpdateOrderTrackDto trackDto)
        {
            // Get the order
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
                return NotFound(new ApiResponse(404, "Order not found"));
            
            // Update track status based on the string value received
            OrderTrackStatus newStatus;
            if (Enum.TryParse(trackDto.TrackStatus, out newStatus))
            {
                order.TrackStatus = newStatus;
                
                // Additional functionality: If TrackStatus is changed to Delivered and current Status is Pending,
                // update the Status to PaymentReceived
                if (newStatus == OrderTrackStatus.Delivered && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.PaymentReceived;
                }
                
                // Save changes
                _unitOfWork.Repository<Order>().Update(order);
                var result = await _unitOfWork.CompleteAsync();
                
                if (result <= 0)
                    return BadRequest(new ApiResponse(400, "Problem updating order tracking status"));
                
                return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
            }
            
            return BadRequest(new ApiResponse(400, "Invalid track status value. Valid values are: Pending, StartOrder, OnTheWay, Delivered"));
        }

        [HttpPut("{deliveryPersonId}/toggle-status")]
        public async Task<ActionResult<bool>> ToggleDeliveryStatus(int deliveryPersonId)
        {
            // Get the delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryPersonId);
            
            if (deliveryMethod == null)
                return NotFound(new ApiResponse(404, "Delivery person not found"));
            
            // Toggle the status
            deliveryMethod.StatusOfDelivery = !deliveryMethod.StatusOfDelivery;
            
            // Save changes
            _unitOfWork.Repository<DeliveryMethod>().Update(deliveryMethod);
            var result = await _unitOfWork.CompleteAsync();
            
            if (result <= 0)
                return BadRequest(new ApiResponse(400, "Problem updating delivery status"));
            
            return Ok(new { 
                success = true,
                statusOfDelivery = deliveryMethod.StatusOfDelivery
            });
        }

        [HttpPut("{deliveryPersonId}/update-info")]
        public async Task<ActionResult<bool>> UpdateDeliveryInfo(int deliveryPersonId, [FromBody] UpdateDeliveryInfoDto updateInfoDto)
        {
            try
            {
                Console.WriteLine($"Debug - Received update request for delivery person {deliveryPersonId}");
                Console.WriteLine($"Debug - DTO values: Description={updateInfoDto.Description}, Cost={updateInfoDto.Cost}, DeliveryTime={updateInfoDto.DeliveryTime}");
                Console.WriteLine($"Debug - DTO address values: Street={updateInfoDto.Street}, City={updateInfoDto.City}, Country={updateInfoDto.Country}, Address={updateInfoDto.Address}");
                Console.WriteLine($"Debug - DTO shift values: StartShift={updateInfoDto.StartShift}, EndShift={updateInfoDto.EndShift}");
                
                // Get the delivery method
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryPersonId);
                
                if (deliveryMethod == null)
                {
                    Console.WriteLine($"Debug - Delivery person {deliveryPersonId} not found");
                    return NotFound(new ApiResponse(404, "Delivery person not found"));
                }
                
                Console.WriteLine($"Debug - Current delivery method values: Description={deliveryMethod.Description}, Cost={deliveryMethod.Cost}, DeliveryTime={deliveryMethod.DeliveryTime}");
                Console.WriteLine($"Debug - Current address values: Street={deliveryMethod.Street}, City={deliveryMethod.City}, Country={deliveryMethod.Country}");
                Console.WriteLine($"Debug - Current shift values: StartShift={deliveryMethod.StartShift}, EndShift={deliveryMethod.EndShift}");
                
                // Always update these fields
                deliveryMethod.Description = updateInfoDto.Description;
                deliveryMethod.Cost = updateInfoDto.Cost;
                deliveryMethod.DeliveryTime = updateInfoDto.DeliveryTime;
                
                // Update address fields if provided (but not required)
                if (!string.IsNullOrEmpty(updateInfoDto.Street))
                {
                    deliveryMethod.Street = updateInfoDto.Street;
                }
                
                if (!string.IsNullOrEmpty(updateInfoDto.City))
                {
                    deliveryMethod.City = updateInfoDto.City;
                }
                
                if (!string.IsNullOrEmpty(updateInfoDto.Country))
                {
                    deliveryMethod.Country = updateInfoDto.Country;
                }
                
                // If Address field was directly provided, use it to update the individual fields
                if (!string.IsNullOrEmpty(updateInfoDto.Address))
                {
                    // The setter of the Address property will parse and update the individual fields
                    deliveryMethod.Address = updateInfoDto.Address;
                    Console.WriteLine($"Debug - Updated address from combined field: Street={deliveryMethod.Street}, City={deliveryMethod.City}, Country={deliveryMethod.Country}");
                }
                
                // Update shift times with better parsing
                if (!string.IsNullOrEmpty(updateInfoDto.StartShift))
                {
                    try
                    {
                        // Try different formats for time parsing
                        TimeSpan startShift;
                        if (TimeSpan.TryParse(updateInfoDto.StartShift, out startShift))
                        {
                            deliveryMethod.StartShift = startShift;
                            Console.WriteLine($"Debug - Parsed StartShift: {startShift}");
                        }
                        else
                        {
                            // Try to parse as DateTime and extract TimeSpan
                            DateTime dateTime;
                            if (DateTime.TryParse(updateInfoDto.StartShift, out dateTime))
                            {
                                startShift = dateTime.TimeOfDay;
                                deliveryMethod.StartShift = startShift;
                                Console.WriteLine($"Debug - Parsed StartShift from DateTime: {startShift}");
                            }
                            else
                            {
                                // Try to parse as HH:mm format
                                var parts = updateInfoDto.StartShift.Split(':');
                                if (parts.Length >= 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                                {
                                    startShift = new TimeSpan(hours, minutes, 0);
                                    deliveryMethod.StartShift = startShift;
                                    Console.WriteLine($"Debug - Parsed StartShift from HH:mm: {startShift}");
                                }
                                else
                                {
                                    Console.WriteLine($"Debug - Failed to parse StartShift: {updateInfoDto.StartShift}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Debug - Error parsing StartShift: {ex.Message}");
                    }
                }
                
                if (!string.IsNullOrEmpty(updateInfoDto.EndShift))
                {
                    try
                    {
                        // Try different formats for time parsing
                        TimeSpan endShift;
                        if (TimeSpan.TryParse(updateInfoDto.EndShift, out endShift))
                        {
                            deliveryMethod.EndShift = endShift;
                            Console.WriteLine($"Debug - Parsed EndShift: {endShift}");
                        }
                        else
                        {
                            // Try to parse as DateTime and extract TimeSpan
                            DateTime dateTime;
                            if (DateTime.TryParse(updateInfoDto.EndShift, out dateTime))
                            {
                                endShift = dateTime.TimeOfDay;
                                deliveryMethod.EndShift = endShift;
                                Console.WriteLine($"Debug - Parsed EndShift from DateTime: {endShift}");
                            }
                            else
                            {
                                // Try to parse as HH:mm format
                                var parts = updateInfoDto.EndShift.Split(':');
                                if (parts.Length >= 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                                {
                                    endShift = new TimeSpan(hours, minutes, 0);
                                    deliveryMethod.EndShift = endShift;
                                    Console.WriteLine($"Debug - Parsed EndShift from HH:mm: {endShift}");
                                }
                                else
                                {
                                    Console.WriteLine($"Debug - Failed to parse EndShift: {updateInfoDto.EndShift}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Debug - Error parsing EndShift: {ex.Message}");
                    }
                }
                
                // Save changes
                Console.WriteLine($"Debug - Updating delivery method in database");
                _unitOfWork.Repository<DeliveryMethod>().Update(deliveryMethod);
                var result = await _unitOfWork.CompleteAsync();
                Console.WriteLine($"Debug - Database update result: {result}");
                
                if (result > 0)
                {
                    var response = new
                    {
                        success = true,
                        deliveryPerson = new
                        {
                            id = deliveryMethod.Id,
                            email = deliveryMethod.Email,
                            shortName = deliveryMethod.ShortName,
                            street = deliveryMethod.Street,
                            city = deliveryMethod.City,
                            country = deliveryMethod.Country,
                            address = string.IsNullOrEmpty(deliveryMethod.Street) ? null : $"{deliveryMethod.Street}, {deliveryMethod.City}, {deliveryMethod.Country}",
                            phoneNumber = deliveryMethod.PhoneNumber,
                            deliveryTime = deliveryMethod.DeliveryTime,
                            description = deliveryMethod.Description,
                            cost = deliveryMethod.Cost,
                            statusOfDelivery = deliveryMethod.StatusOfDelivery,
                            startShift = FormatTimeSpan(deliveryMethod.StartShift),
                            endShift = FormatTimeSpan(deliveryMethod.EndShift)
                        }
                    };
                    
                    Console.WriteLine($"Debug - Returning success response with updated data");
                    return Ok(response);
                }
                
                Console.WriteLine($"Debug - Database update failed");
                return BadRequest(new ApiResponse(400, "Problem updating delivery information"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug - Exception in UpdateDeliveryInfo: {ex.Message}");
                Console.WriteLine($"Debug - Stack trace: {ex.StackTrace}");
                return BadRequest(new ApiResponse(400, $"Error updating delivery information: {ex.Message}"));
            }
        }

        [HttpPost("optimize-route")]
        public async Task<ActionResult<RouteOptimizationResponseDto>> OptimizeDeliveryRoute([FromBody] RouteOptimizationRequestDto request)
        {
            try
            {
                // Get delivery person location
                var deliveryPerson = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(request.DeliveryPersonId);
                if (deliveryPerson == null)
                    return NotFound(new ApiResponse(404, "Delivery person not found"));

                // Get restaurant locations
                var restaurants = new List<(string name, Location location)>();
                foreach (var restaurantId in request.RestaurantIds)
                {
                    var restaurant = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(restaurantId);
                    if (restaurant == null)
                        return NotFound(new ApiResponse(404, $"Restaurant with ID {restaurantId} not found"));

                    Location location;
                    // Check if we have valid coordinates
                    if (Math.Abs(restaurant.Latitude) > 0.000001 || Math.Abs(restaurant.Longitude) > 0.000001)
                    {
                        location = new Location
                        {
                            Latitude = restaurant.Latitude,
                            Longitude = restaurant.Longitude
                        };
                    }
                    else
                    {
                        // Try to geocode the address
                        var address = string.Join(", ", new[] { restaurant.Street, restaurant.City, restaurant.Country }
                            .Where(s => !string.IsNullOrWhiteSpace(s)));
                            
                        if (string.IsNullOrWhiteSpace(address))
                            return BadRequest(new ApiResponse(400, $"Restaurant {restaurant.Name} has no valid address or coordinates"));

                        try
                        {
                            location = await _geocodingService.GetLocationFromAddress(address);
                            
                            // Update restaurant coordinates for future use
                            restaurant.Latitude = location.Latitude;
                            restaurant.Longitude = location.Longitude;
                            await _unitOfWork.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(new ApiResponse(400, $"Failed to geocode address for restaurant {restaurant.Name}: {ex.Message}"));
                        }
                    }
                    
                    restaurants.Add((restaurant.Name, location));
                }

                // Get or geocode delivery person start location
                Location startPoint;
                var deliveryAddress = $"{deliveryPerson.Street}, {deliveryPerson.City}, {deliveryPerson.Country}";
                if (!string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    try
                    {
                        startPoint = await _geocodingService.GetLocationFromAddress(deliveryAddress);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ApiResponse(400, $"Failed to geocode delivery person address: {ex.Message}"));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Delivery person has no valid address"));
                }

                var endPoint = request.CustomerLocation;
                if (Math.Abs(endPoint.Latitude) < 0.000001 && Math.Abs(endPoint.Longitude) < 0.000001)
                {
                    return BadRequest(new ApiResponse(400, "Invalid customer location coordinates"));
                }

                // Optimize route
                var optimizedRoute = await _tspOptimizer.OptimizeRoute(startPoint, endPoint, waypoints: restaurants.Select(r => r.location).ToList());

                // Calculate costs
                var (cost, minutes) = await _costEstimator.EstimateDeliveryCost(optimizedRoute);

                // Create detailed route with names
                var detailedRoute = new List<LocationWithDetails>();

                // Add delivery person start point
                detailedRoute.Add(new LocationWithDetails
                {
                    Name = deliveryPerson.ShortName,
                    Type = "DeliveryPerson",
                    Location = optimizedRoute[0]
                });

                // Add restaurants in optimized order
                for (int i = 1; i < optimizedRoute.Count - 1; i++)
                {
                    var currentPoint = optimizedRoute[i];
                    var matchingRestaurant = restaurants.FirstOrDefault(
                        r => Math.Abs(r.location.Latitude - currentPoint.Latitude) < 0.0001 &&
                             Math.Abs(r.location.Longitude - currentPoint.Longitude) < 0.0001);

                    detailedRoute.Add(new LocationWithDetails
                    {
                        Name = matchingRestaurant.name,
                        Type = "Restaurant",
                        Location = currentPoint
                    });
                }

                // Add customer endpoint
                detailedRoute.Add(new LocationWithDetails
                {
                    Name = "Customer",
                    Type = "Customer",
                    Location = optimizedRoute[optimizedRoute.Count - 1]
                });

                return Ok(new RouteOptimizationResponseDto
                {
                    OptimizedRoute = detailedRoute,
                    EstimatedCost = cost,
                    EstimatedMinutes = minutes
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Route optimization failed: {ex.Message}"));
            }
        }

        [HttpPut("orderitem/{orderItemId}/status")]
        public async Task<ActionResult<bool>> UpdateOrderItemStatus(int orderItemId, [FromBody] OrderItemStatusDto statusDto)
        {
            // Get the order item
            var orderItem = await _unitOfWork.Repository<OrderItem>().GetByIdAsync(orderItemId);
            
            if (orderItem == null)
                return NotFound(new ApiResponse(404, "Order item not found"));
            
            // Update the view status
            orderItem.View = statusDto?.Status ?? "Processing";
            
            // Save changes
            _unitOfWork.Repository<OrderItem>().Update(orderItem);
            var result = await _unitOfWork.CompleteAsync();
            
            if (result <= 0)
                return BadRequest(new ApiResponse(400, "Problem updating order item status"));
            
            return Ok(new { 
                success = true,
                message = $"Order item marked as {orderItem.View}"
            });
        }

        [HttpPost("calculate-delivery-cost")]
        public async Task<ActionResult<decimal>> CalculateDeliveryCost(DeliveryRouteDto routeDto)
        {
            try
            {
                // Get the order
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(routeDto.OrderId);
                if (order == null)
                    return NotFound(new ApiResponse(404, "Order not found"));
            
                // Get delivery method
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(order.DeliveryMethodId);
                if (deliveryMethod == null)
                    return NotFound(new ApiResponse(404, "Delivery method not found"));
            
                // Prepare coordinates
                var deliveryCoordinates = (routeDto.DeliveryLatitude, routeDto.DeliveryLongitude);
            
                var brandCoordinates = routeDto.BrandLocations
                    .Select(b => ((double)b.Latitude, (double)b.Longitude))
                    .ToList();
            
                var customerCoordinates = (routeDto.CustomerLatitude, routeDto.CustomerLongitude);
            
                // Calculate route time
                double routeTimeMinutes = await _routeTimeCalculator.CalculateRouteTimeInMinutesAsync(
                    deliveryCoordinates, brandCoordinates, customerCoordinates);
            
                // Calculate delivery cost based on route time and hourly rate
                decimal deliveryCost = _routeTimeCalculator.CalculateDeliveryCost(routeTimeMinutes, deliveryMethod.Cost);
            
                // Update order with calculated cost
                order.DeliveryCost = deliveryCost;
            
                _unitOfWork.Repository<Order>().Update(order);
                await _unitOfWork.CompleteAsync();
            
                return Ok(new { 
                    orderId = order.Id,
                    routeTimeMinutes,
                    hourlyRate = deliveryMethod.Cost,
                    deliveryCost,
                    total = order.GetTotal() // Include updated total with new delivery cost
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error calculating delivery cost: {ex.Message}"));
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            try
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            catch
            {
                return "00:00"; // Default value if formatting fails
            }
        }
    }
} 
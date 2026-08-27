using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Core.Services;

namespace Talabat.APlS.Controllers
{
    [Authorize]
    public class OrdersController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpPost] // POST: /api/Orders
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var address = _mapper.Map<AddressDto, Address>(orderDto.shipToAddress);

            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, address, orderDto.PaymentMethod);

            if (order is null) return BadRequest(new ApiResponse(400));

            return Ok(order);
        }

        [HttpGet] // GET: /api/Orders
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var orders = await _orderService.GetOrdersForUserAsync(buyerEmail);

            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [HttpGet("{id}")] // GET: /api/Orders/1
        public async Task<ActionResult<OrderToReturnDto>> GetOrderForUser(int id)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var order = await _orderService.GetOrderByIdForUserAsync(buyerEmail, id);

            if (order is null) return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        [HttpGet("deliveryMethods")] // GET: /api/Orders/deliveryMethods
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods(
            [FromQuery] string itemCity = null,
            [FromQuery] string city = null,
            [FromQuery] string country = null)
        {
            try
            {
                // Log request parameters for debugging
                Console.WriteLine($"Debug - GetDeliveryMethods called with itemCity: {itemCity}, city: {city}, country: {country}");
                
                var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
                Console.WriteLine($"Debug - Total delivery methods found: {deliveryMethods.Count}");
                
                // Get current time
                TimeSpan currentTime = DateTime.Now.TimeOfDay;
                
                // الشرط الأول: تصفية خدمات التوصيل المتاحة حسب الوقت أو الحالة
                // Filter to include delivery methods that are either:
                // 1. Marked as available (StatusOfDelivery = true)
                // 2. Current time is within their shift hours
                var availableDeliveryMethods = deliveryMethods.Where(d => 
                    d.StatusOfDelivery || 
                    (d.StartShift != TimeSpan.Zero && d.EndShift != TimeSpan.Zero && 
                     currentTime >= d.StartShift && currentTime <= d.EndShift)
                ).ToList();
                
                Console.WriteLine($"Debug - Available delivery methods after time filtering: {availableDeliveryMethods.Count}");
                
                // If no location parameters are provided, return all available methods
                if (string.IsNullOrEmpty(itemCity) && string.IsNullOrEmpty(city) && string.IsNullOrEmpty(country))
                {
                    return Ok(availableDeliveryMethods);
                }
                
                // For each delivery method, log its address for debugging
                foreach (var dm in availableDeliveryMethods)
                {
                    Console.WriteLine($"Debug - Delivery Method ID: {dm.Id}, Name: {dm.ShortName}, City: {dm.City ?? "null"}, Country: {dm.Country ?? "null"}");
                }
                
                // الشرط الثاني: تصفية خدمات التوصيل حسب الموقع
                // Filter delivery methods based on location match
                var filteredDeliveryMethods = availableDeliveryMethods.Where(d => {
                    try {
                        var deliveryCity = d.City?.Trim().ToLower();
                        var deliveryCountry = d.Country?.Trim().ToLower();
                        var itemCityLower = itemCity?.Trim().ToLower();
                        
                        // إذا لم يكن لدى عامل التوصيل مدينة أو دولة محددة، نعتبره متاح للتوصيل
                        if (string.IsNullOrEmpty(deliveryCity) && string.IsNullOrEmpty(deliveryCountry))
                        {
                            return true;
                        }
                        
                        bool cityMatch = !string.IsNullOrEmpty(deliveryCity) && 
                                      (!string.IsNullOrEmpty(itemCityLower) && 
                                       deliveryCity.Equals(itemCityLower, StringComparison.OrdinalIgnoreCase));
                                        
                        bool countryMatch = !string.IsNullOrEmpty(deliveryCountry) && 
                                         (!string.IsNullOrEmpty(itemCityLower) && 
                                          deliveryCountry.Equals(itemCityLower, StringComparison.OrdinalIgnoreCase));
                           
                        bool result = cityMatch || countryMatch;
                        
                        Console.WriteLine($"Debug - Delivery Method ID: {d.Id}, Name: {d.ShortName}");
                        Console.WriteLine($"  Delivery City: {deliveryCity}, Delivery Country: {deliveryCountry}");
                        Console.WriteLine($"  Item City: {itemCityLower}");
                        Console.WriteLine($"  Match Result: {result} (City Match: {cityMatch}, Country Match: {countryMatch})");
                        
                        return result;
                    }
                    catch (Exception ex) {
                        // Log exception but don't fail the entire request
                        Console.WriteLine($"Error matching delivery method {d.Id}: {ex.Message}");
                        return false; // Don't include this delivery method in case of error
                    }
                }).ToList();
                
                Console.WriteLine($"Debug - Final filtered delivery methods count: {filteredDeliveryMethods.Count}");
                
                // إذا لم يتم العثور على خدمات توصيل متطابقة مع المدينة/الدولة، نعيد قائمة فارغة
                if (filteredDeliveryMethods.Count == 0)
                {
                    Console.WriteLine("Debug - No delivery methods match the restaurant's location, returning empty list");
                    return Ok(new List<DeliveryMethod>()); // إرجاع قائمة فارغة
                }
                
                return Ok(filteredDeliveryMethods);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDeliveryMethods: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<bool>> AuthenticateDeliveryPerson([FromBody] DeliveryAuthDto authDto)
        {
            try
            {
                var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
                var deliveryPerson = deliveryMethods.FirstOrDefault(d => 
                    d.Email == authDto.Email && 
                    d.Password == authDto.Password);

                if (deliveryPerson == null)
                    return Unauthorized(new ApiResponse(401, "Invalid credentials"));

                return Ok(new { 
                    success = true,
                    deliveryPerson = new {
                        id = deliveryPerson.Id,
                        email = deliveryPerson.Email,
                        shortName = deliveryPerson.ShortName,
                        street = deliveryPerson.Street,
                        city = deliveryPerson.City,
                        country = deliveryPerson.Country,
                        phoneNumber = deliveryPerson.PhoneNumber
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AuthenticateDeliveryPerson: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "An error occurred while processing your authentication request" });
            }
        }

        [HttpPost("calculateDeliveryCost")]
        public async Task<ActionResult<DeliveryCostDto>> CalculateDeliveryCost([FromBody] DeliveryCostCalculationDto calculationDto)
        {
            try
            {
                // Get the delivery method
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(calculationDto.DeliveryMethodId);
                if (deliveryMethod == null)
                    return NotFound(new ApiResponse(404, "Delivery method not found"));

                // Get the basket items to find the brands
                var basket = await _orderService.GetBasketByIdAsync(calculationDto.BasketId);
                if (basket == null)
                    return NotFound(new ApiResponse(404, "Basket not found"));

                // Get the shipping address
                var shippingAddress = _mapper.Map<AddressDto, Address>(calculationDto.ShippingAddress);
                if (shippingAddress == null)
                    return BadRequest(new ApiResponse(400, "Invalid shipping address"));

                // Calculate the delivery cost
                var (cost, routeTimeMinutes) = await _orderService.CalculateDeliveryCostAsync(
                    deliveryMethod,
                    basket.Items,
                    shippingAddress
                );

                return Ok(new DeliveryCostDto
                {
                    Cost = cost,
                    RouteTimeMinutes = routeTimeMinutes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating delivery cost: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "An error occurred while calculating delivery cost" });
            }
        }
    }
}

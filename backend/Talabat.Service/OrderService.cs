using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Core.Services;
using Talabat.Core.Specifications;
using Talabat.Core.Specifications.Order_Spec;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IRouteTimeCalculator _routeTimeCalculator;
        private readonly IGeocodingService _geocodingService;

        public OrderService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            IRouteTimeCalculator routeTimeCalculator = null,
            IGeocodingService geocodingService = null)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _routeTimeCalculator = routeTimeCalculator;
            _geocodingService = geocodingService;
        }

        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress, string paymentMethod)
        {
            // 1. Get Basket From Baskets Repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            if (basket == null) return null;

            // 2. Get Selected Items at Basket From Products Repo
            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if (product == null) continue;

                // Get brand information for this product
                var brand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(product.ProductBrandId);

                var productItemOrdered = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity)
                {
                    // Set brand information from the associated ProductBrand
                    BrandName = brand?.Name,
                    BrandStreet = brand?.Street,
                    BrandCity = brand?.City,
                    BrandCountry = brand?.Country
                };
                orderItems.Add(orderItem);
            }

            // 3. Calculate SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // 4. Get Delivery Method From DeliveryMethods Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            if (deliveryMethod == null) return null;

            // 5. Use the pre-calculated delivery cost and route time from the basket
            decimal deliveryCost = basket.ShippingPrice ?? deliveryMethod.Cost;
            int routeTimeMinutes = basket.RouteTimeMinutes ?? 0;

            // If there's no pre-calculated cost, calculate it now (fallback)
            if (deliveryCost == 0 || routeTimeMinutes == 0)
            {
                var calculationResult = await CalculateDeliveryCostAsync(deliveryMethod, basket.Items, shippingAddress);
                deliveryCost = calculationResult.cost;
                routeTimeMinutes = calculationResult.routeTimeMinutes;
            }

            // 6. Create Order
            var spec = new OrderWithPaymentIntentIdSpec(basket.PaymentIntentId);
            var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
            }
            basket = await _paymentService.CreateOrUpdatePaymentIntent(basket.Id);

            // Create the order with restaurant-specific items
            var order = new Order(
                buyerEmail,
                shippingAddress,
                deliveryMethod,
                orderItems,
                subTotal,
                paymentMethod,
                deliveryCost
            )
            {
                PaymentIntentId = basket.PaymentIntentId,
                TrackStatus = OrderTrackStatus.Pending,
                RouteTimeMinutes = routeTimeMinutes
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);

            // 7. Save To Database
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) return null;

            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return deliveryMethods;
        }

        public async Task<Order?> GetOrderByIdForUserAsync(string buyerEmail, int orderId)
        {
            var spec = new OrderByEmailAndIdSpecification(buyerEmail, orderId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrderByEmailSpecification(buyerEmail);
            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            return orders;
        }
        
        public async Task<IReadOnlyList<Order>> GetOrdersForDeliveryPersonAsync(int deliveryPersonId)
        {
            var spec = new OrderByDeliveryPersonIdSpecification(deliveryPersonId);
            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            return orders;
        }
        
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var spec = new BaseSpeceifications<Order>(o => o.Id == orderId);
            spec.Includes.Add(o => o.DeliveryMethod);
            spec.Includes.Add(o => o.Items);
            
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
        }

        public async Task<CustomerBasket> GetBasketByIdAsync(string basketId)
        {
            return await _basketRepo.GetBasketAsync(basketId);
        }

        public async Task<(decimal cost, int routeTimeMinutes)> CalculateDeliveryCostAsync(
            DeliveryMethod deliveryMethod,
            IReadOnlyList<BasketItem> basketItems,
            Address shippingAddress)
        {
            decimal deliveryCost = 0;
            int routeTimeMinutes = 0;

            if (_routeTimeCalculator != null && _geocodingService != null)
            {
                try
                {
                    // Get coordinates for delivery person
                    var deliveryAddress = $"{deliveryMethod.Street}, {deliveryMethod.City}, {deliveryMethod.Country}";
                    var deliveryLocation = await _geocodingService.GetLocationFromAddress(deliveryAddress);
                    
                    // Get coordinates for each restaurant in the order
                    var brandCoordinates = new List<(double, double)>();
                    var uniqueBrands = new HashSet<string>();

                    foreach (var item in basketItems)
                    {
                        // Get product details
                        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                        if (product == null) continue;

                        // Get brand information
                        var brand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(product.ProductBrandId);
                        if (brand == null) continue;

                        var brandName = brand.Name;
                        if (!string.IsNullOrEmpty(brandName) && !uniqueBrands.Contains(brandName))
                        {
                            uniqueBrands.Add(brandName);
                            
                            // Get brand location
                            var brandAddress = $"{brand.Street}, {brand.City}, {brand.Country}";
                            
                            if (!string.IsNullOrEmpty(brandAddress))
                            {
                                var brandLocation = await _geocodingService.GetLocationFromAddress(brandAddress);
                                brandCoordinates.Add((brandLocation.Latitude, brandLocation.Longitude));
                            }
                        }
                    }
                    
                    // Get coordinates for customer
                    var customerAddress = $"{shippingAddress.Street}, {shippingAddress.City}, {shippingAddress.Country}";
                    var customerLocation = await _geocodingService.GetLocationFromAddress(customerAddress);
                    
                    // Calculate route time and delivery cost
                    var deliveryCoordinates = (deliveryLocation.Latitude, deliveryLocation.Longitude);
                    var customerCoordinates = (customerLocation.Latitude, customerLocation.Longitude);
                    
                    routeTimeMinutes = (int)await _routeTimeCalculator.CalculateRouteTimeInMinutesAsync(
                        deliveryCoordinates, brandCoordinates, customerCoordinates);

                    deliveryCost = _routeTimeCalculator.CalculateDeliveryCost(routeTimeMinutes, deliveryMethod.Cost);
                }
                catch (Exception ex)
                {
                    // In case of failure, fall back to the default delivery cost
                    Console.WriteLine($"Error calculating delivery cost automatically: {ex.Message}");
                    deliveryCost = deliveryMethod.Cost;
                }
            }
            else
            {
                // Fall back to the default delivery cost
                deliveryCost = deliveryMethod.Cost;
            }

            return (deliveryCost, routeTimeMinutes);
        }
    }
}

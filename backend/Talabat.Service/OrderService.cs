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
using Talabat.Core.Specifications.Order_Spec;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            // 1. Get Basket From Baskets Repo
            var basket = await _basketRepo.GetBasketAsync(basketId);

            // 2. Get Selected Items at Basket From Products Repo
            var orderItems = new List<OrderItem>();

            if (basket?.Items?.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    var productItemOrdered = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }

            // 3. Calculate SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // 4. Get Delivery Method From DeliveryMethods Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            // 5. Create Order
            var spec = new OrderWithPaymentIntentIdSpec(basket.PaymentIntentId);
            var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }

            var order = new Order(buyerEmail, shippingAddress, deliveryMethod, orderItems, subTotal, basket.PaymentIntentId);
            await _unitOfWork.Repository<Order>().AddAsync(order);

            // 6. Save To Database [TODO]
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
            var spec = new OrderSpecifications(buyerEmail, orderId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrderSpecifications(buyerEmail);
            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            return orders;
        }
    }
}

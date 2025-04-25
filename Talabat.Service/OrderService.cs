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
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(IBasketRepository basketRepository, IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<Order?> CreateOrderAsync(string BuyerEmail, string BasketId, int DeliveryMethodId, Address ShippingAddress)
        {
            var Basket = await _basketRepository.GetBasketAsync(BasketId);
            var OrderItems = new List<OrderItem>();
            if (Basket?.Items.Count > 0)
            {
                foreach (var item in Basket.Items)
                {
                    var Product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    var ProductItemOrdered = new ProductItemOrdered(Product.Id, Product.Name, Product.PictureUrl);
                    var OrderItem = new OrderItem(ProductItemOrdered, Product.Price, item.Quantity);
                    OrderItems.Add(OrderItem);

                }
            }
            
            var SubTotal = OrderItems.Sum(item => item.Price * item.Quantity);
            var DeliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(DeliveryMethodId);
            var Spec = new OrderWithPaymentIntentIdSpec(Basket.PaymentIntentId);
            var ExOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(Spec);
            if (ExOrder is not null)
            {
                _unitOfWork.Repository<Order>().Delete(ExOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(BasketId);

            }
            var Order = new Order(BuyerEmail, ShippingAddress, DeliveryMethod, OrderItems, SubTotal, Basket.PaymentIntentId);
            await _unitOfWork.Repository<Order>().AddAsync(Order);
            var Result=await _unitOfWork.CompleteAsync();
            if (Result <= 0) return null;
            return Order;
        }

        public async Task<Order?> GetOrderByIdForSpecificUserAsync(string BuyerEmail, int OrderId)
        {
            var Spec = new OrderSpecifications(BuyerEmail, OrderId);
            var Order= await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(Spec);
            return Order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForSpecificUserAsync(string BuyerEmail)
        {
            var Spec = new OrderSpecifications(BuyerEmail);
            var Orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(Spec);
            return Orders;
        }
    }
}

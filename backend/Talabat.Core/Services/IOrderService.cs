using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Core.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress, string paymentMethod);
        Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
        Task<Order?> GetOrderByIdForUserAsync(string buyerEmail, int orderId);
        Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
        Task<IReadOnlyList<Order>> GetOrdersForDeliveryPersonAsync(int deliveryPersonId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<CustomerBasket> GetBasketByIdAsync(string basketId);
        Task<(decimal cost, int routeTimeMinutes)> CalculateDeliveryCostAsync(
            DeliveryMethod deliveryMethod,
            IReadOnlyList<BasketItem> basketItems,
            Address shippingAddress);
    }
}

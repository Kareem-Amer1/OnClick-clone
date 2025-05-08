using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Core.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string BuyerEmail, string BasketId, int DeliveryMethodId, Address ShippingAddress);
        Task<IReadOnlyList<Order>> GetOrdersForSpecificUserAsync(string BuyerEmail);
        Task<Order?> GetOrderByIdForSpecificUserAsync( string BuyerEmail,int OrderId);
        //Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
        //Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail, OrderSpecParams specParams);
        //Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
    }
}

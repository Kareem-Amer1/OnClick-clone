using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
        }

        public Order(string buyerEmail, Address shippingAddress, DeliveryMethod deliveryMethod, List<OrderItem> items, decimal subTotal, string paymentMethod, decimal deliveryCost = 0)
        {
            BuyerEmail = buyerEmail;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            SubTotal = subTotal;
            PaymentMethod = paymentMethod;
            DeliveryCost = deliveryCost;
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public OrderTrackStatus TrackStatus { get; set; } = OrderTrackStatus.Pending;
        public Address ShippingAddress { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public decimal DeliveryCost { get; set; }
        public int DeliveryMethodId { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal SubTotal { get; set; }
        public string PaymentIntentId { get; set; }
        public string PaymentMethod { get; set; }
        public int? RouteTimeMinutes { get; set; }
        public decimal GetTotal()
        {
            return SubTotal + DeliveryCost;
        }
    }
}

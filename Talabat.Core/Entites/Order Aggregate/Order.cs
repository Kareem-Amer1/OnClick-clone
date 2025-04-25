using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public class Order : BaseEntity
    {
        public Order(string buyerEmail, Address shipingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subTotal, string paymentIntentId)
        {
            BuyerEmail = buyerEmail;
            ShipingAddress = shipingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            SubTotal = subTotal;
            PaymentIntentId = paymentIntentId;
        }

        public Order()
        {
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Address ShipingAddress { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        //public int DeliveryMethodId { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
        public decimal SubTotal { get; set; }
        //[NotMapped]
        //public decimal Total { get => SubTotal + DeliveryMethod.Cost; }
        public decimal GetTotal()
        => SubTotal+ DeliveryMethod.Cost;

        public string PaymentIntentId { get; set; } 


    }
}

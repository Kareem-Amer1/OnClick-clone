using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.APlS.DTOs
{
    public class OrderToReturnDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string Status { get; set; }
        public string TrackStatus { get; set; }
        public Address ShippingAddress { get; set; }
        public string DeliveryMethod { get; set; }
        public decimal DeliveryCost { get; set; }
        public IReadOnlyList<OrderItemDto> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string PaymentIntentId { get; set; }
        public string PaymentMethod { get; set; }
        public bool HasItemsFromMultipleRestaurants { get; set; }
        public int? RouteTimeMinutes { get; set; }
    }
}

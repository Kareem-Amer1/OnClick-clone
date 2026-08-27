using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.APlS.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public string BrandName { get; set; }
        public string BrandStreet { get; set; }
        public string BrandCity { get; set; }
        public string BrandCountry { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string View { get; set; }
        public int OrderItemId { get; set; }
    }
}
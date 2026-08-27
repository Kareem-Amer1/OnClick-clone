using System;

namespace Talabat.APlS.DTOs
{
    public class BrandToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string CuisineType { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public bool? IsAvailable { get; set; }
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
        public string Token { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites
{
    public class ProductBrand : BaseEntity
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        
        // Authentication properties
        public string Email { get; set; }
        public string Password { get; set; }
        
        // Restaurant specific properties
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string CuisineType { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; } // in minutes
        public bool? IsAvailable { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
    }
}

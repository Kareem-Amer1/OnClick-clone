using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class DeliveryRouteDto
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public double DeliveryLatitude { get; set; }
        
        [Required]
        public double DeliveryLongitude { get; set; }
        
        [Required]
        public List<BrandLocationDto> BrandLocations { get; set; } = new List<BrandLocationDto>();
        
        [Required]
        public double CustomerLatitude { get; set; }
        
        [Required]
        public double CustomerLongitude { get; set; }
    }
    
    public class BrandLocationDto
    {
        [Required]
        public int BrandId { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
    }
} 
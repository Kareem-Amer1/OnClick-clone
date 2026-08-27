using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Talabat.Core.Entites;

namespace Talabat.APlS.DTOs
{
    public class RouteOptimizationRequestDto
    {
        [Required]
        public List<int> RestaurantIds { get; set; }

        [Required]
        public Location CustomerLocation { get; set; }

        [Required]
        public int DeliveryPersonId { get; set; }
    }

    public class RouteOptimizationResponseDto
    {
        public List<LocationWithDetails> OptimizedRoute { get; set; }
        public decimal EstimatedCost { get; set; }
        public int EstimatedMinutes { get; set; }
    }

    public class LocationWithDetails
    {
        public string Name { get; set; }
        public string Type { get; set; } // "DeliveryPerson", "Restaurant", or "Customer"
        public Location Location { get; set; }
    }
} 
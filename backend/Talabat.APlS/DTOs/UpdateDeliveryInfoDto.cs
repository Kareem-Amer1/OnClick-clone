using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class UpdateDeliveryInfoDto
    {
        [Required]
        public string Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be greater than or equal to 0")]
        public decimal Cost { get; set; }
        
        public string StartShift { get; set; }
        
        public string EndShift { get; set; }
        
        // Address fields (optional)
        public string Street { get; set; }
        
        public string City { get; set; }
        
        public string Country { get; set; }
        
        // Main address field
        public string Address { get; set; }
        
        [Required]
        public string DeliveryTime { get; set; }
    }
} 
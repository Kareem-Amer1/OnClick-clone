using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class DeliveryCostCalculationDto
    {
        [Required]
        public int DeliveryMethodId { get; set; }

        [Required]
        public string BasketId { get; set; }

        [Required]
        public AddressDto ShippingAddress { get; set; }
    }
} 
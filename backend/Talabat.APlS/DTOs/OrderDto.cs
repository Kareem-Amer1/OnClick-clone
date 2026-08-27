using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; }
        [Required]
        public int DeliveryMethodId { get; set; }
        [Required]
        public AddressDto shipToAddress { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}

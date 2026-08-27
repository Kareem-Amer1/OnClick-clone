using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class ProductUpdateDto
    {
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
        
        public string PictureUrl { get; set; }
        
        public int ProductTypeId { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class BrandLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
} 
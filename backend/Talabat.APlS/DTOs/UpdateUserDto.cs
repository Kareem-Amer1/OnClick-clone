using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public string DisplayName { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
} 
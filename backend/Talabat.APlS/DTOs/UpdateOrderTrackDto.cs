using System.ComponentModel.DataAnnotations;

namespace Talabat.APlS.DTOs
{
    public class UpdateOrderTrackDto
    {
        [Required]
        public string TrackStatus { get; set; } = "Pending";
    }
} 
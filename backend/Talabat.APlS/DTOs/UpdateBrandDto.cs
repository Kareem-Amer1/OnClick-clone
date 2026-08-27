using System;

namespace Talabat.APlS.DTOs
{
    public class UpdateBrandDto
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
    }
} 
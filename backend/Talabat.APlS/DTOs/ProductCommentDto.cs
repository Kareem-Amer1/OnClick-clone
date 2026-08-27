using System;

namespace Talabat.APlS.DTOs
{
    public class ProductCommentDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Rating { get; set; }
        public int ProductId { get; set; }
    }

    public class CreateProductCommentDto
    {
        public string Comment { get; set; }
        public int Rating { get; set; }
    }
} 
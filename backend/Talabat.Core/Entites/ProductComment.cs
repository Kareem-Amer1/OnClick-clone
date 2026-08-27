using System;

namespace Talabat.Core.Entites
{
    public class ProductComment : BaseEntity
    {
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Rating { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
} 
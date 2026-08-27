using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public class OrderItem : BaseEntity
    {
        public OrderItem()
        {
            View = "Pending"; // Default value
        }

        public OrderItem(ProductItemOrdered product, decimal price, int quantity)
        {
            Product = product;
            Price = price;
            Quantity = quantity;
            View = "Pending"; // Default value
        }

        public ProductItemOrdered Product { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string View { get; set; } // Pending or Delivered
        public string BrandName { get; set; } // Restaurant/Brand name
        public string BrandStreet { get; set; } // Restaurant/Brand street address
        public string BrandCity { get; set; } // Restaurant/Brand city
        public string BrandCountry { get; set; } // Restaurant/Brand country
        //public int? RestaurantOrderId { get; set; }
        //public RestaurantOrder RestaurantOrder { get; set; }
    }
}

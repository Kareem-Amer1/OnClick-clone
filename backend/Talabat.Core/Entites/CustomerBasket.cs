using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites
{
    public class CustomerBasket
    {
        public CustomerBasket(string id)
        {
            Id = id;
            Items = new List<BasketItem>();
        }
        
        public string Id { get; set; }
        public List<BasketItem> Items { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public int? DeliveryMethodId { get; set; }
        public decimal? ShippingPrice { get; set; }
        public int? RouteTimeMinutes { get; set; }

        public decimal GetSubTotal()
        {
            return Items.Sum(item => item.Price * item.Quantity);
        }

        public bool HasItemsFromMultipleRestaurants()
        {
            return false; // This functionality is removed with the simplified basket
        }
        
        public bool HasItemsFromMultipleCities()
        {
            if (Items == null || Items.Count <= 1)
                return false;
                
            // Get the first item's city
            var firstCity = Items.FirstOrDefault()?.BrandCity;
            
            // Check if all items have the same city
            return Items.Any(item => !string.Equals(item.BrandCity, firstCity, StringComparison.OrdinalIgnoreCase));
        }
        
        public string GetFirstCity()
        {
            return Items?.FirstOrDefault()?.BrandCity ?? string.Empty;
        }
    }
}

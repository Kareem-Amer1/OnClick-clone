using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public class DeliveryMethod : BaseEntity
    {
        public DeliveryMethod()
        {
            StatusOfDelivery = false;
        }

        public DeliveryMethod(string shortName, string deliveryTime, string description, decimal cost, string email, string password, string street, string city, string country, string phoneNumber)
        {
            ShortName = shortName;
            DeliveryTime = deliveryTime;
            Description = description;
            Cost = cost;
            Email = email;
            Password = password;
            Street = street;
            City = city;
            Country = country;
            PhoneNumber = phoneNumber;
            StatusOfDelivery = false;
        }

        //public int Id { get; set; }
        public string ShortName { get; set; }
        public string DeliveryTime { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public bool StatusOfDelivery { get; set; }
        
        // Address properties
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        
        [Obsolete("Use individual address properties instead")]
        public string Address
        {
            get {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(Street)) parts.Add(Street);
                if (!string.IsNullOrEmpty(City)) parts.Add(City);
                if (!string.IsNullOrEmpty(Country)) parts.Add(Country);
                
                if (parts.Count == 0)
                    return string.Empty;
                    
                return string.Join(", ", parts);
            }
            set { /* This is kept for backward compatibility */ } 
        }
        
        public TimeSpan StartShift { get; set; }
        public TimeSpan EndShift { get; set; }
    }
}

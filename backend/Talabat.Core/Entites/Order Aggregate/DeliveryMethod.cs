using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public class DeliveryMethod : BaseEntity
    {
        public DeliveryMethod()
        {
        }

        public DeliveryMethod(string shortName, string deliveryTime, string description, decimal cost, string email, string password, string address, string phoneNumber)
        {
            ShortName = shortName;
            DeliveryTime = deliveryTime;
            Description = description;
            Cost = cost;
            Email = email;
            Password = password;
            Address = address;
            PhoneNumber = phoneNumber;
        }

        //public int Id { get; set; }
        public string ShortName { get; set; }
        public string DeliveryTime { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}

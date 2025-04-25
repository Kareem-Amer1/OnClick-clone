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

        public DeliveryMethod(string shortName, string deliveryTime, string description, decimal cost)
        {
            ShortName = shortName;
            DeliveryTime = deliveryTime;
            Description = description;
            Cost = cost;
        }

        //public int Id { get; set; }
        public string ShortName { get; set; }
        public string DeliveryTime { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
       
    }
}

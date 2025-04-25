using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Core.Specifications.Order_Spec
{
    public class OrderSpecifications : BaseSpeceifications<Order>
    {
        public OrderSpecifications(string email):base(O=>O.BuyerEmail== email)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
            AddOrderByDescending(O => O.OrderDate);
        }
        public OrderSpecifications(string email,int id) : base(O => O.Id == id && O.BuyerEmail == email)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
            //AddOrderByDescending(O => O.OrderDate);
        }
    }
}

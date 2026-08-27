using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Core.Specifications.Order_Spec
{
    public class OrderByEmailSpecification : BaseSpeceifications<Order>
    {
        public OrderByEmailSpecification(string email) : base(O => O.BuyerEmail == email)
        {
            Includes.Add(O => O.DeliveryMethod);
            AddOrderByDescending(O => O.OrderDate);
        }
    }

    public class OrderByEmailAndIdSpecification : BaseSpeceifications<Order>
    {
        public OrderByEmailAndIdSpecification(string email, int id) : base(O => O.Id == id && O.BuyerEmail == email)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
        }
    }
    
    public class OrderByDeliveryPersonIdSpecification : BaseSpeceifications<Order>
    {
        public OrderByDeliveryPersonIdSpecification(int deliveryPersonId) : base(O => O.DeliveryMethod.Id == deliveryPersonId && O.TrackStatus != OrderTrackStatus.Delivered)
        {
            Includes.Add(O => O.DeliveryMethod);
            AddOrderByDescending(O => O.OrderDate);
        }
    }
} 
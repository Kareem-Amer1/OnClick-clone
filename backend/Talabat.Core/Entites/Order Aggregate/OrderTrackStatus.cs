using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entites.Order_Aggregate
{
    public enum OrderTrackStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        
        [EnumMember(Value = "Start Order")]
        StartOrder,
        
        [EnumMember(Value = "On The Way")]
        OnTheWay,
        
        [EnumMember(Value = "Delivered")]
        Delivered
    }
} 
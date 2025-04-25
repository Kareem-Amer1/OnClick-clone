using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Repository.Data.Configurations
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(O => O.Status)
            .HasConversion(OStatus => OStatus.ToString(), OStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), OStatus));
            
            builder.Property(O => O.SubTotal).HasColumnType("decimal(18,2)");
            builder.OwnsOne(O => O.ShipingAddress, X=>X.WithOwner());
            builder.HasOne(O => O.DeliveryMethod).WithMany().OnDelete(DeleteBehavior.NoAction);
        }
    }
}

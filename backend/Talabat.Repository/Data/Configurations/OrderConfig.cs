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
            
            builder.Property(O => O.TrackStatus)
            .HasConversion(OTrack => OTrack.ToString(), OTrack => (OrderTrackStatus)Enum.Parse(typeof(OrderTrackStatus), OTrack));
            
            builder.Property(O => O.SubTotal).HasColumnType("decimal(18,2)");
            builder.Property(O => O.DeliveryCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(O => O.PaymentMethod).IsRequired();
            builder.OwnsOne(O => O.ShippingAddress, X=>X.WithOwner());
            builder.HasOne(O => O.DeliveryMethod).WithMany().OnDelete(DeleteBehavior.NoAction);
        }
    }
}

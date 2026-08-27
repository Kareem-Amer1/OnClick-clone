using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Repository.Data.Configurations
{
    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(OI => OI.Price)
                .HasColumnType("decimal(18,2)");
            builder.OwnsOne(OI=> OI.Product, P=>P.WithOwner());
            
            builder.Property(OI => OI.View)
                .HasDefaultValue("Pending");
                
            // Configure brand information properties
            builder.Property(OI => OI.BrandName)
                .HasMaxLength(100)
                .IsRequired(false); // Optional, can be null
                
            builder.Property(OI => OI.BrandStreet)
                .HasMaxLength(200)
                .IsRequired(false);
                
            builder.Property(OI => OI.BrandCity)
                .HasMaxLength(100)
                .IsRequired(false);
                
            builder.Property(OI => OI.BrandCountry)
                .HasMaxLength(100)
                .IsRequired(false);
        }
    }
}

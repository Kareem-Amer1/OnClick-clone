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
    public class DeliveryMethodConfig : IEntityTypeConfiguration<DeliveryMethod>
    {
        public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.Property(DM => DM.Cost).HasColumnType("decimal(18,2)");
            builder.Property(DM => DM.Email).IsRequired();
            builder.Property(DM => DM.Password).IsRequired();
            
            // Configure address fields as optional
            builder.Property(DM => DM.Street).IsRequired(false);
            builder.Property(DM => DM.City).IsRequired(false);
            builder.Property(DM => DM.Country).IsRequired(false);
            
            // Configure the obsolete Address property to be ignored by EF
            builder.Ignore(DM => DM.Address);
            
            builder.Property(DM => DM.PhoneNumber).IsRequired();
            builder.Property(DM => DM.StatusOfDelivery).HasDefaultValue(false);
            builder.Property(DM => DM.StartShift).HasColumnType("time");
            builder.Property(DM => DM.EndShift).HasColumnType("time");
        }
    }
}

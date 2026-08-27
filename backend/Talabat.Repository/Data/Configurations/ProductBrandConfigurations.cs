using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entites;

namespace Talabat.Repository.Data.Configurations
{
    internal class ProductBrandConfigurations : IEntityTypeConfiguration<ProductBrand>
    {
        public void Configure(EntityTypeBuilder<ProductBrand> builder)
        {
            builder.Property(B => B.Name).IsRequired();
            builder.Property(B => B.Street).IsRequired(false);
            builder.Property(B => B.City).IsRequired(false);
            builder.Property(B => B.Country).IsRequired(false);
            
            // Authentication configurations
            builder.Property(B => B.Email).IsRequired(false);
            builder.Property(B => B.Password).IsRequired(false);
            
            // Restaurant specific configurations
            builder.Property(B => B.Description).IsRequired(false);
            builder.Property(B => B.LogoUrl).IsRequired(false);
            builder.Property(B => B.CuisineType).IsRequired(false);
            builder.Property(B => B.MinOrderAmount).HasColumnType("decimal(18,2)");
            builder.Property(B => B.DeliveryFee).HasColumnType("decimal(18,2)");
            builder.Property(B => B.EstimatedDeliveryTime).HasDefaultValue(45);
            builder.Property(B => B.IsAvailable).HasDefaultValue(true);
            builder.Property(B => B.Rating).HasDefaultValue(0);
            builder.Property(B => B.TotalRatings).HasDefaultValue(0);
            builder.Property(B => B.Latitude).HasDefaultValue(0);
            builder.Property(B => B.Longitude).HasDefaultValue(0);
        }
    }
    
}

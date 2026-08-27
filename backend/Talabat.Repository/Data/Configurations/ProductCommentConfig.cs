using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entites;

namespace Talabat.Repository.Data.Configurations
{
    public class ProductCommentConfig : IEntityTypeConfiguration<ProductComment>
    {
        public void Configure(EntityTypeBuilder<ProductComment> builder)
        {
            builder.Property(pc => pc.UserEmail)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pc => pc.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pc => pc.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(pc => pc.Rating)
                .IsRequired();

            builder.HasOne(pc => pc.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 
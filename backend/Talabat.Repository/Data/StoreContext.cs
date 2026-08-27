using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Repository.Data.Configurations;

namespace Talabat.Repository.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new ProductConfigurations());
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<RestaurantOrder>()
            //    .HasOne(ro => ro.Order)
            //    .WithMany(o => o.RestaurantOrders)
            //    .HasForeignKey(ro => ro.OrderId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<RestaurantOrder>()
            //    .Property(ro => ro.SubTotal)
            //    .HasPrecision(18, 2);

            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.RestaurantOrder)
            //    .WithMany(ro => ro.Items)
            //    .HasForeignKey(oi => oi.RestaurantOrderId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        //public DbSet<RestaurantOrder> RestaurantOrders { get; set; }
    }
}

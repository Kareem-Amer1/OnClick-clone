using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.Repository.Data
{
    public static class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext dbContext)
        {
            // Update or Add ProductBrands
            var BrandsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/brands.json");
            var Brands = JsonSerializer.Deserialize<List<ProductBrand>>(BrandsData);
            if (Brands?.Count > 0)
            {
                foreach (var Brand in Brands)
                {
                    var existingBrand = await dbContext.ProductBrands
                        .FirstOrDefaultAsync(b => b.Name == Brand.Name);
                    
                    if (existingBrand != null)
                    {
                        // Update existing brand
                        existingBrand.Street = Brand.Street;
                        existingBrand.City = Brand.City;
                        existingBrand.Country = Brand.Country;
                        dbContext.ProductBrands.Update(existingBrand);
                    }
                    else
                    {
                        // Add new brand
                        await dbContext.ProductBrands.AddAsync(Brand);
                    }
                }
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.ProductTypes.Any())
            {
                var TypesData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/types.json");
                var Types = JsonSerializer.Deserialize<List<ProductType>>(TypesData);
                if (Types?.Count > 0)
                {
                    foreach (var Type in Types)
                    {
                        await dbContext.Set<ProductType>().AddAsync(Type);

                    }
                    await dbContext.SaveChangesAsync();
                }
            }

            if (!dbContext.Products.Any())
            {
                var ProductsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/products.json");
                var Products = JsonSerializer.Deserialize<List<Product>>(ProductsData);
                if (Products?.Count > 0)
                {
                    foreach (var Product in Products)
                    {
                        await dbContext.Set<Product>().AddAsync(Product);

                    }
                    await dbContext.SaveChangesAsync();
                }
            }

            if (!dbContext.DeliveryMethods.Any())
            {
                var DeliveryMethodsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/delivery.json");
                var DeliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(DeliveryMethodsData);
                if (DeliveryMethods?.Count > 0)
                {
                    foreach (var DeliveryMethod in DeliveryMethods)
                    {
                        await dbContext.Set<DeliveryMethod>().AddAsync(DeliveryMethod);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            else
            {
                // Update existing delivery methods with new address fields
                var DeliveryMethodsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/delivery.json");
                var DeliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(DeliveryMethodsData);
                if (DeliveryMethods?.Count > 0)
                {
                    foreach (var DeliveryMethod in DeliveryMethods)
                    {
                        var existingDeliveryMethod = await dbContext.DeliveryMethods
                            .FirstOrDefaultAsync(d => d.Email == DeliveryMethod.Email);
                        
                        if (existingDeliveryMethod != null)
                        {
                            // Update address fields
                            existingDeliveryMethod.Street = DeliveryMethod.Street;
                            existingDeliveryMethod.City = DeliveryMethod.City;
                            existingDeliveryMethod.Country = DeliveryMethod.Country;
                            
                            // Update other fields
                            existingDeliveryMethod.Description = DeliveryMethod.Description;
                            existingDeliveryMethod.DeliveryTime = DeliveryMethod.DeliveryTime;
                            existingDeliveryMethod.Cost = DeliveryMethod.Cost;
                            
                            dbContext.DeliveryMethods.Update(existingDeliveryMethod);
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}

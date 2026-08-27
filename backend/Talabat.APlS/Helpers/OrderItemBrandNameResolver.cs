using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Talabat.APlS.DTOs;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Repository.Data;

namespace Talabat.APlS.Helpers
{
    public class OrderItemBrandNameResolver : IValueResolver<OrderItem, OrderItemDto, string>
    {
        private readonly StoreContext _dbContext;

        public OrderItemBrandNameResolver(StoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string Resolve(OrderItem source, OrderItemDto destination, string destMember, ResolutionContext context)
        {
            var productBrand = _dbContext.Products
                .Where(p => p.Id == source.Product.ProductId)
                .Select(p => p.ProductBrand)
                .FirstOrDefault();

            if (productBrand != null)
            {
                destination.BrandStreet = productBrand.Street;
                destination.BrandCity = productBrand.City;
                destination.BrandCountry = productBrand.Country;
                return productBrand.Name;
            }

            return "Unknown Brand";
        }
    }
} 
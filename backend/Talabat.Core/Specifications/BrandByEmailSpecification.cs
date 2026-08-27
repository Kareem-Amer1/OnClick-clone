using System;
using Talabat.Core.Entites;

namespace Talabat.Core.Specifications
{
    public class BrandByEmailSpecification : BaseSpeceifications<ProductBrand>
    {
        public BrandByEmailSpecification(string email) 
            : base(brand => brand.Email != null && email != null && 
                  brand.Email.ToLower() == email.ToLower())
        {
            // No includes needed for simple email lookup
        }
    }
} 
using Talabat.Core.Entites;
using Talabat.Core.Specifications;

namespace Talabat.APlS.Specifications
{
    public class BrandByEmailSpecification : BaseSpeceifications<ProductBrand>
    {
        public BrandByEmailSpecification(string email)
            : base(b => b.Email == email)
        {
        }
    }
} 
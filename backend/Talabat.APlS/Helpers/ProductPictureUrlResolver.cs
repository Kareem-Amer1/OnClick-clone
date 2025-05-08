using AutoMapper;
using Microsoft.Extensions.Configuration;
using Talabat.APlS.DTOs;
using Talabat.Core.Entites;

namespace Talabat.APlS.Helpers
{
    public class ProductPictureUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
    {
        private readonly IConfiguration _configuration;
        public ProductPictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Product source, ProductToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.pictureUrl))
            {
                return $"{_configuration["ApiBaseUrl"]}{ source.pictureUrl}";
            }
            return string.Empty;
        }
    }
}

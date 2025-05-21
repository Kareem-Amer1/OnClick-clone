using AutoMapper;
using Talabat.APlS.DTOs;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Identity;
using Talabat.Core.Entites.Order_Aggregate;

namespace Talabat.APlS.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.ProductBrand, o => o.MapFrom(s => s.ProductBrand.Name))
                .ForMember(d => d.ProductType, o => o.MapFrom(s => s.ProductType.Name))
                .ForMember(d => d.PictureUrl, o => o.MapFrom<ProductPictureUrlResolver>());
            CreateMap<Core.Entites.Identity.Address, AddressDto>().ReverseMap();
            CreateMap<CustomerBasketDto, CustomerBasket>();//.ReverseMap();
            CreateMap<BasketItemDto, BasketItem>();
            CreateMap<AddressDto, Core.Entites.Order_Aggregate.Address>();
            CreateMap<Order, OrderToReturnDto>().ForMember(d => d.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost));

            CreateMap<OrderItem,OrderItemDto>().ForMember(d => d.ProductId, O => O.MapFrom(S => S.Product.ProductId))
                .ForMember(d => d.ProductName, O => O.MapFrom(S => S.Product.ProductName))
                .ForMember(d => d.PictureUrl, O => O.MapFrom(S => S.Product.PictureUrl))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>());
        }
    }
}

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
                .ForMember(d => d.PictureUrl, o => o.MapFrom<ProductPictureUrlResolver>())
                .ForMember(d => d.BrandCity, o => o.MapFrom(s => s.ProductBrand.City));

            CreateMap<ProductBrand, BrandToReturnDto>()
                .ForMember(d => d.OpeningTime, o => o.MapFrom<TimeSpanToStringResolver>())
                .ForMember(d => d.ClosingTime, o => o.MapFrom<ClosingTimeResolver>())
                .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.IsAvailable.HasValue ? s.IsAvailable : true))
                .ForMember(d => d.LogoUrl, o => o.MapFrom<BrandLogoUrlResolver>());
                
            CreateMap<ProductBrand, ProductBrandDto>();
            CreateMap<Core.Entites.Identity.Address, AddressDto>().ReverseMap();
            CreateMap<CustomerBasketDto, CustomerBasket>();//.ReverseMap();
            CreateMap<BasketItemDto, BasketItem>();
            CreateMap<AddressDto, Core.Entites.Order_Aggregate.Address>();
            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, o => o.MapFrom(s => s.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryCost, o => o.MapFrom(s => s.DeliveryCost))
                .ForMember(d => d.SubTotal, o => o.MapFrom(s => s.SubTotal))
                .ForMember(d => d.Total, o => o.MapFrom(s => s.GetTotal()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.TrackStatus, o => o.MapFrom(s => s.TrackStatus.ToString()))
                .ForMember(d => d.ShippingAddress, o => o.MapFrom(s => s.ShippingAddress))
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod))
                .ForMember(d => d.RouteTimeMinutes, o => o.MapFrom(s => s.RouteTimeMinutes))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
                //.ForMember(d => d.RestaurantOrders, o => o.MapFrom<OrderRestaurantOrdersResolver>());
            
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Product.ProductId))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.Product.PictureUrl))
                .ForMember(d => d.BrandName, o => o.MapFrom<OrderItemBrandNameResolver>())
                .ForMember(d => d.PictureUrl, o => o.MapFrom<OrderItemPictureUrlResolver>());

            CreateMap<ProductComment, ProductCommentDto>();
        }
    }
}

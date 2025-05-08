using Microsoft.AspNetCore.Mvc;
using Talabat.APlS.Errors;
using Talabat.APlS.Helpers;
using Talabat.Core;
using Talabat.Core.Repositories;
using Talabat.Core.Services;
using Talabat.Repository;
using Talabat.Service;

namespace Talabat.APlS.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection Services)
        {
            Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            Services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //builder.Services.AddAutoMapper(M=>M.AddProfile(new MappingProfiles()));
            Services.AddAutoMapper(typeof(MappingProfiles));
            Services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.InvalidModelStateResponseFactory = (ActionContext) =>
                {
                    var errors = ActionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                        .SelectMany(P => P.Value.Errors)
                        .Select(E => E.ErrorMessage).ToArray();
                    var ValidationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(ValidationErrorResponse);
                };
            });
            Services.AddScoped<IUnitOfWork, UnitOfWork>();
            Services.AddScoped<IOrderService, OrderService>();
            Services.AddScoped<IPaymentService, PaymentService>();
            return Services;
        }
    }
}

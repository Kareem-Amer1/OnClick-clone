using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Talabat.Core.Entites.Identity;
using Talabat.Core.Services;
using Talabat.Repository.Identity;
using Talabat.Service;

namespace Talabat.APlS.Extensions
{
    public static class IdentityServicesExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddScoped<ITokenService, TokenService>();

            Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            Services.AddAuthentication(Option =>
            {
                Option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                };
            });
            return Services;
        }
    }
}

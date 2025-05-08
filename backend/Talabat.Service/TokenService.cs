using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Talabat.Core.Entites.Identity;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> CreateTokenAsync(AppUser User,UserManager<AppUser> userManager)
        {
            var AuthClaim = new List<Claim>()  
            {
                new Claim(ClaimTypes.GivenName, User.DisplayName),
                new Claim(ClaimTypes.Email, User.Email)

            };

            var UserRoles =  await  userManager.GetRolesAsync(User) ;
            foreach (var Role in UserRoles)
            {
                AuthClaim.Add(new Claim(ClaimTypes.Role, Role));
            }

            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var Token = new JwtSecurityToken(
                issuer : configuration["JWT:ValidIssuer"],
                audience : configuration["JWT:ValidAudience"],
                expires : DateTime.Now.AddDays(double.Parse(configuration["JWT:DurationInDays"])),
                claims : AuthClaim,
                signingCredentials : new SigningCredentials(AuthKey,SecurityAlgorithms.HmacSha256Signature)
            );
            return new JwtSecurityTokenHandler().WriteToken(Token);

        }
    }
}

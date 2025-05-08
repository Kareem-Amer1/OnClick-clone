using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entites.Identity;

namespace Talabat.APlS.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task<AppUser?> FindUserWithAdressAsync(this UserManager<AppUser> userManager, ClaimsPrincipal User)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user =  await userManager.Users.Include(U => U.Address).FirstOrDefaultAsync(U => U.Email == email);
            return user;
        }
    }
}

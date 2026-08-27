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
            
            // Use explicit selection to avoid the column name mapping issue
            var user = await userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Email == email);
                
            return user;
        }
    }
}

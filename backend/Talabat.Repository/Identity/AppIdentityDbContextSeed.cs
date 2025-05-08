using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entites.Identity;
using Talabat.Repository.Identity.Migrations;

namespace Talabat.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var User = new AppUser()
                {
                    DisplayName = "kareem amer",
                    Email = "kareem2003@gmail.com",
                    UserName = "kareem2003",
                    PhoneNumber = "01281824877",
                };
                await userManager.CreateAsync(User, "Pa$$w0rd");
            }
            
            
        }
    } 
}

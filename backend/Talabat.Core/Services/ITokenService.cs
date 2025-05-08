using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entites.Identity;

namespace Talabat.Core.Services
{
    public interface ITokenService {
    
        Task<string>CreateTokenAsync(AppUser User, UserManager<AppUser> userManager);
    }
}

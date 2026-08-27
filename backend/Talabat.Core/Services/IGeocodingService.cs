using System.Threading.Tasks;
using Talabat.Core.Entites;

namespace Talabat.Core.Services
{
    public interface IGeocodingService
    {
        Task<Location> GetLocationFromAddress(string address);
        Task<string> GetAddressFromLocation(Location location);
    }
} 
using System.Threading.Tasks;
using Talabat.Core.Entites;

namespace Talabat.Core.Services
{
    public interface IDistanceService
    {
        Task<double> GetDistance(Location origin, Location destination);
        Task<double> GetDuration(Location origin, Location destination);
        Task<(double distance, double duration)> GetDistanceAndDuration(Location origin, Location destination);
    }
} 
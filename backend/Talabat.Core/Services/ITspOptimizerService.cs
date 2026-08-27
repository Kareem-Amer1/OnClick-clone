using System.Collections.Generic;
using System.Threading.Tasks;
using Talabat.Core.Entites;

namespace Talabat.Core.Services
{
    public interface ITspOptimizerService
    {
        Task<List<Location>> OptimizeRoute(Location startPoint, Location endPoint, List<Location> waypoints);
        double CalculateTotalDistance(List<Location> route);
    }
} 
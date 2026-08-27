using System.Collections.Generic;
using System.Threading.Tasks;
using Talabat.Core.Entites;

namespace Talabat.Core.Services
{
    public interface IDeliveryCostEstimator
    {
        Task<(decimal cost, int estimatedMinutes)> EstimateDeliveryCost(List<Location> route);
    }
} 
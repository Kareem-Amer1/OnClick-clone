using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabat.Core.Entites;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class DeliveryCostEstimator : IDeliveryCostEstimator
    {
        private readonly IDistanceService _distanceService;
        
        // Egyptian market rates
        private const decimal BASE_COST = 25.0m;        // Base cost in EGP
        private const decimal COST_PER_KM = 3.0m;       // Cost per kilometer in EGP
        private const decimal COST_PER_MINUTE = 1.0m;   // Cost per minute in EGP
        private const decimal MULTI_RESTAURANT_FEE = 5.0m; // Additional fee per restaurant after first one

        public DeliveryCostEstimator(IDistanceService distanceService)
        {
            _distanceService = distanceService;
        }

        public async Task<(decimal cost, int estimatedMinutes)> EstimateDeliveryCost(List<Location> route)
        {
            if (route == null || route.Count < 2)
                throw new ArgumentException("Route must contain at least 2 points");

            double totalDistance = 0;
            double totalDuration = 0;

            // Calculate total distance and duration using actual road distances
            for (int i = 0; i < route.Count - 1; i++)
            {
                var result = await _distanceService.GetDistanceAndDuration(route[i], route[i + 1]);
                totalDistance += result.distance;
                totalDuration += result.duration;
            }

            // Calculate costs
            var distanceCost = (decimal)totalDistance * COST_PER_KM;
            var timeCost = (decimal)totalDuration * COST_PER_MINUTE;
            
            // Additional fee for multiple restaurants (route.Count - 2 because first and last points are delivery person and customer)
            var multiRestaurantCost = (route.Count - 3) * MULTI_RESTAURANT_FEE; // -3 because we don't count first, last, and first restaurant

            var totalCost = BASE_COST + distanceCost + timeCost + (multiRestaurantCost > 0 ? multiRestaurantCost : 0);

            // Round up to nearest 0.5 EGP
            totalCost = Math.Ceiling(totalCost * 2) / 2;

            return (totalCost, (int)Math.Ceiling(totalDuration));
        }
    }
} 
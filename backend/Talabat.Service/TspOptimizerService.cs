using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talabat.Core.Entites;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class TspOptimizerService : ITspOptimizerService
    {
        private readonly IDistanceService _distanceService;
        private const int MAX_ITERATIONS = 1000;

        public TspOptimizerService(IDistanceService distanceService)
        {
            _distanceService = distanceService;
        }

        public async Task<List<Location>> OptimizeRoute(Location startPoint, Location endPoint, List<Location> waypoints)
        {
            if (waypoints == null || !waypoints.Any())
                return new List<Location> { startPoint, endPoint };

            // Create initial route: start -> waypoints -> end
            var route = new List<Location> { startPoint };
            route.AddRange(waypoints);
            route.Add(endPoint);

            // Build distance matrix
            var n = route.Count;
            var distances = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        var result = await _distanceService.GetDistance(route[i], route[j]);
                        distances[i, j] = result;
                    }
                }
            }

            var bestRoute = route.ToList();
            var bestDistance = await CalculateTotalDistanceAsync(bestRoute);
            var improved = true;
            var iterations = 0;

            while (improved && iterations < MAX_ITERATIONS)
            {
                improved = false;
                iterations++;

                // Try all possible 2-opt swaps
                for (int i = 1; i < route.Count - 2; i++)
                {
                    for (int j = i + 1; j < route.Count - 1; j++)
                    {
                        // Skip if points are adjacent
                        if (j - i == 1) continue;

                        // Create new route with 2-opt swap
                        var newRoute = TwoOptSwap(route, i, j);
                        
                        // Calculate new route distance using the pre-calculated distance matrix
                        var newDistance = 0.0;
                        for (int k = 0; k < newRoute.Count - 1; k++)
                        {
                            var fromIndex = route.IndexOf(newRoute[k]);
                            var toIndex = route.IndexOf(newRoute[k + 1]);
                            newDistance += distances[fromIndex, toIndex];
                        }

                        // If new route is better, keep it
                        if (newDistance < bestDistance)
                        {
                            bestRoute = newRoute;
                            bestDistance = newDistance;
                            improved = true;
                        }
                    }
                }

                if (improved)
                {
                    route = bestRoute;
                }
            }

            return route;
        }

        private List<Location> TwoOptSwap(List<Location> route, int i, int j)
        {
            var newRoute = new List<Location>();
            
            // Add all points up to i
            for (int k = 0; k <= i - 1; k++)
            {
                newRoute.Add(route[k]);
            }
            
            // Add points from i to j in reverse order
            for (int k = j; k >= i; k--)
            {
                newRoute.Add(route[k]);
            }
            
            // Add all points after j
            for (int k = j + 1; k < route.Count; k++)
            {
                newRoute.Add(route[k]);
            }
            
            return newRoute;
        }

        private async Task<double> CalculateTotalDistanceAsync(List<Location> route)
        {
            double totalDistance = 0;
            
            for (int i = 0; i < route.Count - 1; i++)
            {
                var distance = await _distanceService.GetDistance(route[i], route[i + 1]);
                totalDistance += distance;
            }
            
            return totalDistance;
        }

        // Implementation of interface method - uses direct distance calculation
        public double CalculateTotalDistance(List<Location> route)
        {
            if (route == null || route.Count < 2)
                return 0;

            double totalDistance = 0;
            
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += route[i].CalculateDistance(route[i + 1]);
            }
            
            return totalDistance;
        }
    }
} 
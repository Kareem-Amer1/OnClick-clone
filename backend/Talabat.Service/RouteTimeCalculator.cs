using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class RouteTimeCalculator : IRouteTimeCalculator
    {
        private readonly HttpClient _httpClient;
        private readonly string _googleMapsApiKey;

        // Average speed in km/h for delivery vehicles in urban areas
        private const double AverageSpeedKmh = 30;

        public RouteTimeCalculator(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _googleMapsApiKey = configuration["GoogleMaps:ApiKey"];
        }

        public async Task<double> CalculateRouteTimeInMinutesAsync(
            (double Latitude, double Longitude) deliveryCoordinates,
            List<(double Latitude, double Longitude)> brandCoordinates,
            (double Latitude, double Longitude) customerCoordinates)
        {
            try
            {
                // For actual implementation, we would use Google Maps Distance Matrix API
                // But for demonstration, we'll use a simplified calculation based on distance
                
                // Create the full route: delivery person -> all brands -> customer
                var fullRoute = new List<(double Latitude, double Longitude)> { deliveryCoordinates };
                fullRoute.AddRange(brandCoordinates);
                fullRoute.Add(customerCoordinates);
                
                double totalDistanceKm = 0;
                
                // Calculate the total distance of the route
                for (int i = 0; i < fullRoute.Count - 1; i++)
                {
                    var pointA = fullRoute[i];
                    var pointB = fullRoute[i + 1];
                    totalDistanceKm += CalculateHaversineDistance(pointA.Latitude, pointA.Longitude, 
                                                              pointB.Latitude, pointB.Longitude);
                }
                
                // Estimate time based on average speed (minutes = distance / speed * 60)
                double estimatedTimeMinutes = (totalDistanceKm / AverageSpeedKmh) * 60;
                
                // Add time for each stop (5 minutes per brand for pickup)
                estimatedTimeMinutes += brandCoordinates.Count * 5;
                
                return estimatedTimeMinutes;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error calculating route time: {ex.Message}");
                
                // Return a default estimate if calculation fails
                return 45; // Default to 45 minutes
            }
        }
        
        public decimal CalculateDeliveryCost(double routeTimeInMinutes, decimal hourlyRate)
        {
            // Convert minutes to hours and multiply by hourly rate
            decimal hoursDecimal = (decimal)(routeTimeInMinutes / 60);
            return hoursDecimal * hourlyRate;
        }
        
        /// <summary>
        /// Calculates the distance between two coordinates using the Haversine formula
        /// </summary>
        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Earth radius in kilometers
            const double R = 6371;
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in km
        }
        
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 
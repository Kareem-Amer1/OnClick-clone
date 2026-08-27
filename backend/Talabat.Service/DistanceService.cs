using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Talabat.Core.Entites;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class DistanceService : IDistanceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ConcurrentDictionary<string, (double distance, double duration, DateTime timestamp)> _cache;
        private const int CACHE_DURATION_MINUTES = 30;

        public DistanceService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"];
            _cache = new ConcurrentDictionary<string, (double, double, DateTime)>();
        }

        public async Task<double> GetDistance(Location origin, Location destination)
        {
            var result = await GetDistanceAndDuration(origin, destination);
            return result.distance;
        }

        public async Task<double> GetDuration(Location origin, Location destination)
        {
            var result = await GetDistanceAndDuration(origin, destination);
            return result.duration;
        }

        public async Task<(double distance, double duration)> GetDistanceAndDuration(Location origin, Location destination)
        {
            var cacheKey = $"{origin.Latitude},{origin.Longitude}-{destination.Latitude},{destination.Longitude}";

            // Check cache
            if (_cache.TryGetValue(cacheKey, out var cachedValue))
            {
                if (DateTime.UtcNow.Subtract(cachedValue.timestamp).TotalMinutes < CACHE_DURATION_MINUTES)
                {
                    return (cachedValue.distance, cachedValue.duration);
                }
            }

            // Call Google Distance Matrix API
            var url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                     $"?origins={origin.Latitude},{origin.Longitude}" +
                     $"&destinations={destination.Latitude},{destination.Longitude}" +
                     $"&mode=driving" +
                     $"&key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["status"].ToString() != "OK")
            {
                throw new Exception($"Google Distance Matrix API error: {json["status"]}");
            }

            var element = json["rows"][0]["elements"][0];
            if (element["status"].ToString() != "OK")
            {
                throw new Exception($"Route calculation error: {element["status"]}");
            }

            var distance = element["distance"]["value"].Value<double>() / 1000; // Convert meters to kilometers
            var duration = element["duration"]["value"].Value<double>() / 60; // Convert seconds to minutes

            // Update cache
            _cache.AddOrUpdate(cacheKey, 
                (distance, duration, DateTime.UtcNow),
                (key, old) => (distance, duration, DateTime.UtcNow));

            return (distance, duration);
        }
    }
} 
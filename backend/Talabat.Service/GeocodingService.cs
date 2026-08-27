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
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ConcurrentDictionary<string, (Location location, DateTime timestamp)> _locationCache;
        private readonly ConcurrentDictionary<string, (string address, DateTime timestamp)> _addressCache;
        private const int CACHE_DURATION_HOURS = 24;

        public GeocodingService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"];
            _locationCache = new ConcurrentDictionary<string, (Location, DateTime)>();
            _addressCache = new ConcurrentDictionary<string, (string, DateTime)>();
        }

        public async Task<Location> GetLocationFromAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be empty");

            // Check cache
            if (_locationCache.TryGetValue(address, out var cachedLocation))
            {
                if (DateTime.UtcNow.Subtract(cachedLocation.timestamp).TotalHours < CACHE_DURATION_HOURS)
                {
                    return cachedLocation.location;
                }
            }

            try
            {
                // Call Google Geocoding API
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                if (json["status"].ToString() != "OK")
                {
                    if (json["status"].ToString() == "ZERO_RESULTS")
                    {
                        throw new Exception($"No location found for address: {address}");
                    }
                    throw new Exception($"Geocoding API error: {json["status"]}");
                }

                var location = new Location
                {
                    Latitude = json["results"][0]["geometry"]["location"]["lat"].Value<double>(),
                    Longitude = json["results"][0]["geometry"]["location"]["lng"].Value<double>()
                };

                // Update cache
                _locationCache.AddOrUpdate(address,
                    (location, DateTime.UtcNow),
                    (key, old) => (location, DateTime.UtcNow));

                return location;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to geocode address: {address}. Error: {ex.Message}");
            }
        }

        public async Task<string> GetAddressFromLocation(Location location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (Math.Abs(location.Latitude) < 0.000001 && Math.Abs(location.Longitude) < 0.000001)
                throw new ArgumentException("Invalid coordinates: both latitude and longitude are zero");

            var cacheKey = $"{location.Latitude},{location.Longitude}";

            // Check cache
            if (_addressCache.TryGetValue(cacheKey, out var cachedAddress))
            {
                if (DateTime.UtcNow.Subtract(cachedAddress.timestamp).TotalHours < CACHE_DURATION_HOURS)
                {
                    return cachedAddress.address;
                }
            }

            try
            {
                // Call Google Reverse Geocoding API
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={location.Latitude},{location.Longitude}&key={_apiKey}";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                if (json["status"].ToString() != "OK")
                {
                    throw new Exception($"Reverse geocoding API error: {json["status"]}");
                }

                var address = json["results"][0]["formatted_address"].ToString();

                // Update cache
                _addressCache.AddOrUpdate(cacheKey,
                    (address, DateTime.UtcNow),
                    (key, old) => (address, DateTime.UtcNow));

                return address;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reverse geocode location: ({location.Latitude}, {location.Longitude}). Error: {ex.Message}");
            }
        }
    }
} 
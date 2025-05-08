using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class ResponseCacheService: IResponseCacheService
    {
        private readonly IDatabase _database;
        public ResponseCacheService(IConnectionMultiplexer Redis)
        {
            _database = Redis.GetDatabase();
        }

        public async Task CacheResponseAsync(string CacheKey, object Response, TimeSpan ExpireTime)
        {
            if (Response is null) return;// Task.CompletedTask;
            var Options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //WriteIndented = true
            };
            var SerializedResponse = JsonSerializer.Serialize(Response, Options);
            await _database.StringSetAsync(CacheKey, SerializedResponse, ExpireTime);
        }
        public async Task<string?> GetCachedResponse(string CacheKey)
        {
            var CachedResponse= await _database.StringGetAsync(CacheKey);
            if (CachedResponse.IsNullOrEmpty) return null;
            return CachedResponse;
        }
        
    }
  
}

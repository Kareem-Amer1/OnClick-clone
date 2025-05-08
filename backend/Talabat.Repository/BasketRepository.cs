using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;

namespace Talabat.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _database;

        public BasketRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task<bool> DeleteBasketAsync(string BasketId)
        {
            return await _database.KeyDeleteAsync(BasketId);
        }

        public async Task<CustomerBasket?> GetBasketAsync(string BasketId)
        {
            var Basket = await _database.StringGetAsync(BasketId);
            //if (Basket.IsNull) { return null; }
            //else
            //{
            //    var ReturnedBasket = JsonSerializer.Deserialize<CustomerBasket>(Basket);
            //}
            return Basket.IsNull ? null : JsonSerializer.Deserialize<CustomerBasket>(Basket);
        }

        public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket Basket)
        {
            var jsonBasket = JsonSerializer.Serialize(Basket);
            var CreatedOrUpdated=  await _database.StringSetAsync(Basket.Id, jsonBasket, TimeSpan.FromDays(1));
            if (!CreatedOrUpdated) { return null; }
            return await GetBasketAsync(Basket.Id); 
        }
    }
}

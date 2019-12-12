using System.Threading.Tasks;
using Day12.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Day12
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisService(IOptions<RedisOption> options)
        { 
            _redis = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
        }


        public async Task<bool> Set(string key, string value)
        {
            var db = _redis.GetDatabase();
            return await db.StringSetAsync(key, value);
        }

        public async Task<string> Get(string key)
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }
    }
}
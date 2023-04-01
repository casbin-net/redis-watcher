using Casbin.Watcher.Redis.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis.Extensions
{
    public static class MessageExtensions
    {
        public static RedisValue ToRedisValue(this Message message)
        {
            return (RedisValue)JsonConvert.SerializeObject(message);
        }
    }
}
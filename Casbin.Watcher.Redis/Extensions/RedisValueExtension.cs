using Casbin.Watcher.Redis.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis.Extensions
{
    public static class RedisValueExtension
    {
        public static TMessage ToMessage<TMessage>(this RedisValue redisValue) where TMessage : IMessage
        {
            return JsonConvert.DeserializeObject<TMessage>(redisValue.ToString());
        }
    }
}
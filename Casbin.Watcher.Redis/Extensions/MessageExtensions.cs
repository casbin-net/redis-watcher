using System.Linq;
using System.Text.Json;
using Casbin.Watcher.Redis.Entities;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis;

internal static class MessageExtensions
{
    public static RedisValue ToRedisValue(this Message message)
    {
        // IPolicyValues and the readonly lists do not serialize well
        message.SerializableValues = message.Values?.ToList();
        message.SerializableNewValues = message.NewValues?.ToList();
        message.SerializableValuesList = message.ValuesList?.ToList();
        message.SerializableNewValuesList = message.NewValuesList?.ToList();

        return (RedisValue)JsonSerializer.Serialize(message);
    }
}
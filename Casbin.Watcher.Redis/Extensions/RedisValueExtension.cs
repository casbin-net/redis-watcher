using System.Text.Json;
using Casbin.Model;
using Casbin.Watcher.Redis.Entities;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis
{
    internal static class RedisValueExtension
    {
        public static Message ToMessage(this RedisValue redisValue)
        {
            var message = JsonSerializer.Deserialize<Message>(redisValue.ToString());

            message.Values = message.SerializableValues == null ? null : Policy.ValuesFrom(message.SerializableValues);
            message.NewValues = message.SerializableNewValues == null ? null : Policy.ValuesFrom(message.SerializableNewValues);
            message.ValuesList = message.SerializableValuesList == null ? null : Policy.ValuesListFrom(message.SerializableValuesList);
            message.NewValuesList = message.SerializableNewValuesList == null ? null : Policy.ValuesListFrom(message.SerializableNewValuesList);

            return message;
        }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casbin.Model;
using Casbin.Persist;

namespace Casbin.Watcher.Redis.Entities
{
    internal class Message : IPolicyChangeMessage
    {
        public string Id { get; set; }

        public PolicyOperation Operation { get; set; }

        public string Section { get; set; }

        public string PolicyType { get; set; }

        public int FieldIndex { get; set; }

        [JsonIgnore]
        public IPolicyValues Values { get; set; }

        [JsonIgnore]
        public IPolicyValues NewValues { get; set; }

        [JsonIgnore]
        public IReadOnlyList<IPolicyValues> ValuesList { get; set; }

        [JsonIgnore]
        public IReadOnlyList<IPolicyValues> NewValuesList { get; set; }

        // IPolicyValues and the readonly lists do not serialize well, these properties are used to facilitate the serialization.

        public IEnumerable<string> SerializableValues { get; set; }

        public IEnumerable<string> SerializableNewValues { get; set; }

        public IEnumerable<IEnumerable<string>> SerializableValuesList { get; set; }

        public IEnumerable<IEnumerable<string>> SerializableNewValuesList { get; set; }
    }
}
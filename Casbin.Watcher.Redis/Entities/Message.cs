using System.Collections.Generic;

namespace Casbin.Watcher.Redis.Entities
{
    public class Message : IMessage
    {
        public MethodType Method { get; set; }
        public string Id { get; set; }
        public string Sec { get; set; }
        public string Ptype { get; set; }
        public int FieldIndex { get; set; } = -1;
        public IEnumerable<string> Params { get; set; }
    }
}
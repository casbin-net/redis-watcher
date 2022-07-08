using System.Collections.Generic;

namespace Casbin.Watcher.Redis
{
    public interface IMessage
    {
        public MethodType Method { get; set; }
        public string Id { get; set; }
        public string Sec { get; set; }
        public string Ptype { get; set; }
        public int FieldIndex { get; set; }
        public IEnumerable<string> Params { get; set; }
    }
}
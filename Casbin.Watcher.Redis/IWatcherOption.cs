namespace Casbin.Watcher.Redis
{
    public interface IWatcherOption
    {
        public bool Async { get; set; } 
        public string Channel { get; set; } 
        public bool IgnoreSelf { get; set; } 
        public string LocalId { get; set; }
    }
}
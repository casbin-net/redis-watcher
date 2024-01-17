namespace Casbin.Watcher.Redis
{
    public class WatcherOptions : IWatcherOptions
    {
        public bool Async { get; set; }
        public string Channel { get; set; }
        public bool IgnoreSelf { get; set; } = true;
        public string LocalId { get; set; }
    }
}
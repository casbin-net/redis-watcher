namespace Casbin.Watcher.Redis
{
    public interface IWatcherOptions
    {
        bool Async { get; set; }
        string Channel { get; set; }
        bool IgnoreSelf { get; set; }
        string LocalId { get; set; }
    }
}
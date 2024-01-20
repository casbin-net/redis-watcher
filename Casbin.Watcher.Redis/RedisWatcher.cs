using System;
using System.Threading.Tasks;
using Casbin.Persist;
using Casbin.Watcher.Redis.Entities;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis;

public class RedisWatcher : IWatcher
{
    private readonly IConnectionMultiplexer _connection;
    private readonly RedisChannel _channel;
    private readonly IWatcherOptions _options;

    private ISubscriber _subscriber;
    private Action _callback;
    private Func<Task> _asyncCallback;
    private Action<IPolicyChangeMessage> _callbackWithMessage;
    private Func<IPolicyChangeMessage, Task> _asyncCallbackWithMessage;

    public RedisWatcher(string addr = "localhost", IWatcherOptions options = null)
    {
        _options = options ?? new WatcherOptions();

        Id = _options.LocalId ?? Guid.NewGuid().ToString();
        _channel = new RedisChannel(_options.Channel ?? "/casbin", RedisChannel.PatternMode.Literal);
        _connection = ConnectionMultiplexer.Connect(addr);

        if (_options.Async)
            SubscribeAsync();
        else
            Subscribe();
    }

    ~RedisWatcher() => Close();

    public string Id { get; private set; }

    #region IFullWatcher

    public virtual void SetUpdateCallback(Action callback)
    {
        _callback = callback;
    }

    public virtual void SetUpdateCallback(Func<Task> callback)
    {
        _asyncCallback = callback;
    }

    public virtual void Update()
    {
        var message = new Message
        {
            Operation = PolicyOperation.SavePolicy,
            Id = Id,
        };

        _subscriber.Publish(_channel, message.ToRedisValue());
    }

    public virtual async Task UpdateAsync()
    {
        var message = new Message
        {
            Operation = PolicyOperation.SavePolicy,
            Id = Id,
        };

        await _subscriber.PublishAsync(_channel, message.ToRedisValue());
    }

    #endregion

    #region IIncrementalWatcher

    public virtual void SetUpdateCallback(Action<IPolicyChangeMessage> callback)
    {
        _callbackWithMessage = callback;
    }

    public virtual void SetUpdateCallback(Func<IPolicyChangeMessage, Task> callback)
    {
        _asyncCallbackWithMessage = callback;
    }

    public virtual void Update(IPolicyChangeMessage policyMessage)
    {
        var message = new Message
        {
            Id = Id,
            Operation = policyMessage.Operation,
            Section = policyMessage.Section,
            PolicyType = policyMessage.PolicyType,
            FieldIndex = policyMessage.FieldIndex,
            Values = policyMessage.Values,
            NewValues = policyMessage.NewValues,
            ValuesList = policyMessage.ValuesList,
            NewValuesList = policyMessage.NewValuesList
        };

        _subscriber.Publish(_channel, message.ToRedisValue());
    }

    public virtual async Task UpdateAsync(IPolicyChangeMessage policyMessage)
    {
        var message = new Message
        {
            Id = Id,
            Operation = policyMessage.Operation,
            Section = policyMessage.Section,
            PolicyType = policyMessage.PolicyType,
            FieldIndex = policyMessage.FieldIndex,
            Values = policyMessage.Values,
            NewValues = policyMessage.NewValues,
            ValuesList = policyMessage.ValuesList,
            NewValuesList = policyMessage.NewValuesList
        };

        await _subscriber.PublishAsync(_channel, message.ToRedisValue());
    }

    #endregion

    #region IReadOnlyWatcher

    public virtual void Close()
    {
        _subscriber?.UnsubscribeAll();
        _connection?.Close();
    }

    public virtual async Task CloseAsync()
    {
        await _subscriber?.UnsubscribeAllAsync();
        await _connection?.CloseAsync();
    }

    #endregion

    private void Subscribe()
    {
        _subscriber = _connection.GetSubscriber();

        _subscriber.Subscribe(_channel, (RedisChannel _, RedisValue value) =>
        {
            var message = value.ToMessage();
            var isSelf = message.Id == Id;
            if (!(isSelf && _options.IgnoreSelf))
            {
                try
                {
                    if (_callbackWithMessage != null)
                    {
                        _callbackWithMessage.Invoke(message);
                    }
                    else
                    {
                        _callback?.Invoke();
                    }
                }
                catch {}
            }
        });
    }

    private void SubscribeAsync()
    {
        _subscriber = _connection.GetSubscriber();

        _subscriber.Subscribe(_channel, (RedisChannel _, RedisValue value) =>
        {
            var message = value.ToMessage();
            var isSelf = message.Id == Id;
            if (!(isSelf && _options.IgnoreSelf))
            {
                try
                {
                    if (_asyncCallbackWithMessage != null)
                    {
                        _asyncCallbackWithMessage.Invoke(message);
                    }
                    else
                    {
                        _asyncCallback?.Invoke();
                    }
                }
                catch {}
            }
        });
    }
}
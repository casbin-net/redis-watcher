using System;
using System.Threading.Tasks;
using Casbin.Persist;
using Casbin.Watcher.Redis.Entities;
using Casbin.Watcher.Redis.Extensions;
using StackExchange.Redis;

namespace Casbin.Watcher.Redis
{
    public class RedisWatcher : RedisWatcher<Message>
        
    {
        public RedisWatcher(string addr = "localhost") : base(addr)
        {
            
        }
        
        public RedisWatcher(string addr, IWatcherOption option) : base(addr, option)
        {
            
        }
    }
    
    public class RedisWatcher<TMessage> : IWatcher
        where TMessage : IMessage
    {
        public IWatcherOption WatcherOption { get; }
        private readonly IConnectionMultiplexer _connection;
        private ISubscriber _subscriber;
        
        private Action _callback;
        private Func<Task> _asyncCallback;
        private Action<TMessage> _callbackWithMessage;
        private Func<TMessage, Task> _asyncCallbackWithMessage;
        
        private readonly RedisChannel _channel = "/casbin";

        public RedisWatcher(string addr = "localhost")
        {
            WatcherOption = new WatcherOption();
            WatcherOption.LocalId = Guid.NewGuid().ToString();
            WatcherOption.Channel = _channel;

            _connection = ConnectionMultiplexer.Connect(addr);

            Subscribe();
        }

        public RedisWatcher(string addr, IWatcherOption option)
        {
            WatcherOption = option;
            WatcherOption.LocalId ??= Guid.NewGuid().ToString();
            if (WatcherOption.Channel is null)
            {
                WatcherOption.Channel = _channel;
            }
            else
            {
                _channel = option.Channel;
            }

            _connection = ConnectionMultiplexer.Connect(addr);
            if (WatcherOption.Async)
            {
                SubscribeAsync();
            }
            else
            {
                Subscribe();
            }
        }

        ~RedisWatcher() => Close();

        private void Subscribe()
        {
            _subscriber = _connection.GetSubscriber();

            _subscriber.Subscribe(_channel, (RedisChannel _, RedisValue value) =>
            {
                var message = value.ToMessage<TMessage>();
                var isSelf = message.Id == WatcherOption.LocalId;
                if (!(isSelf && WatcherOption.IgnoreSelf))
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
            });
        }

        private void SubscribeAsync()
        {
            _subscriber = _connection.GetSubscriber();
            
            _subscriber.Subscribe(_channel, (RedisChannel _, RedisValue value) =>
            {
                var message = value.ToMessage<TMessage>();
                var isSelf = message.Id == WatcherOption.LocalId;
                if (!(isSelf && WatcherOption.IgnoreSelf))
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
            });
        }
        
        public virtual void SetUpdateCallback(Action callback)
        {
            _callback = callback;
        }

        public virtual void SetUpdateCallback(Func<Task> callback)
        {
            _asyncCallback = callback;
        }
        
        public virtual void SetUpdateCallback(Action<TMessage> callback)
        {
            _callbackWithMessage = callback;
        }
        
        public virtual void SetUpdateCallback(Func<TMessage, Task> callback)
        {
            _asyncCallbackWithMessage = callback;
        }
        
        public virtual void Update()
        {
            var message = new Message
            {
                Method = MethodType.Update, Id = WatcherOption.LocalId
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateAsync()
        {
            var message = new Message
            {
                Method = MethodType.Update, Id = WatcherOption.LocalId
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }

        public virtual void UpdateForAddPolicy(string sec, string ptype, params string[] param)
        {
            var message = new Message
            {
                Method = MethodType.UpdateForAddPolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = param
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateForAddPolicyAsync(string sec, string ptype, params string[] param)
        {
            var message = new Message
            {
                Method = MethodType.UpdateForAddPolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = param
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void UpdateForRemovePolicy(string sec, string ptype, params string[] param) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemovePolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = param
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateForRemovePolicyAsync(string sec, string ptype, params string[] param) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemovePolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = param
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void UpdateForRemoveFilteredPolicy(string sec, string ptype, int fieldIndex, params string[] fieldValues) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemoveFilteredPolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, FieldIndex = fieldIndex, Params = fieldValues
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateForRemoveFilteredPolicyAsync(string sec, string ptype, int fieldIndex, params string[] fieldValues) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemoveFilteredPolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, FieldIndex = fieldIndex, Params = fieldValues
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void UpdateForSavePolicy(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForSavePolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateForSavePolicyAsync(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForSavePolicy, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void UpdateForAddPolicies(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForAddPolicies, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }
        
        public virtual async Task UpdateForAddPoliciesAsync(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForAddPolicies, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void UpdateForRemovePolicies(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemovePolicies, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            _subscriber.Publish(_channel, message.ToRedisValue());
        }

        public virtual async Task UpdateForRemovePoliciesAsync(string sec, string ptype, params string[] rules) 
        {
            var message = new Message
            {
                Method = MethodType.UpdateForRemovePolicies, Id = WatcherOption.LocalId, Sec = sec, Ptype = ptype, Params = rules
            };
            await _subscriber.PublishAsync(_channel, message.ToRedisValue());
        }
        
        public virtual void Close()
        {
            _subscriber?.UnsubscribeAll();
            _connection?.Close();
        }
    }
}

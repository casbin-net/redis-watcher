using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Casbin.Watcher.Redis.Entities;
using Xunit;

namespace Casbin.Watcher.Redis.UnitTest
{
    public class GetMessageTest
    {
        private const int WaitTime = 500;
        
        [Fact]
        public void TestUpdate()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);            
            watcher.SetUpdateCallback(msg => message = msg);
            
            watcher.Update();
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }

        [Fact]
        public void TestWatcherWithIgnoreSelfTrue()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher, new WatcherOption{IgnoreSelf = true});            
            watcher.SetUpdateCallback(msg => message = msg);
            
            watcher.Update();
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForAddPolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.AddPolicy("alice", "book1", "write");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForAddExistentPolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.AddPolicy("alice", "data1", "read");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemovePolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.RemovePolicy("alice", "data1", "read");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemoveNonExistentPolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.RemovePolicy("alice", "data4", "read");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemoveFilteredPolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.RemoveFilteredPolicy(1, "data1", "read");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemoveFilteredNonExistentPolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.RemoveFilteredPolicy(1, "data3", "read");
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateSavePolicy()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);
            
            enforcer.SavePolicy();
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });           
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForAddPolicies()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);

            enforcer.AddPolicies(new []
            {
                new List<string>{"jack", "data4", "read"},
                new List<string>{"katy", "data4", "write"},
                new List<string>{"leyo", "data4", "read"},
                new List<string>{"ham", "data4", "write"},
            });
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });            
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForAddAnyExistentPolicies()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);

            enforcer.AddPolicies(new []
            {
                new List<string>{"data2_admin", "data3", "read"},
                new List<string>{"alice", "data1", "read"},  // Existent
            });
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemovePolicies()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);

            enforcer.RemovePolicies(new []
            {
                new List<string>{"jack", "data4", "read"},
                new List<string>{"katy", "data4", "write"},
                new List<string>{"data2_admin", "data2", "write"},
                new List<string>{"ham", "data4", "write"},
            });
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.Update, Id = watcher.WatcherOption.LocalId, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }
        
        [Fact]
        public void TestUpdateForRemoveNonExistentPolicies()
        {
            IMessage message;
            RedisWatcher watcher;
            IEnforcer enforcer = InitWatcher(out message, out watcher);
            watcher.SetUpdateCallback(msg => message = msg);

            enforcer.RemovePolicies(new []
            {
                new List<string>{"jack", "data4", "read"},
                new List<string>{"katy", "data4", "write"},
                new List<string>{"leyo", "data4", "read"},
                new List<string>{"ham", "data4", "write"},
            });
            Thread.Sleep(WaitTime);
            MessageEquals(message, new Message
            {
                Method = MethodType.None, Id = null, Sec = null, Ptype = null, FieldIndex = -1, Params = null
            });
            watcher.Close();
        }

        private static IEnforcer InitWatcher(out IMessage message, out RedisWatcher watcher, WatcherOption option = null, bool cluster = false)
        {
            message = new Message();
            watcher = option is null ? new RedisWatcher() : new RedisWatcher("localhost", option);
            IEnforcer enforcer = new Enforcer("examples/rbac_model.conf", "examples/rbac_policy.csv");
            enforcer.SetWatcher(watcher);
            return enforcer;
        }

        private bool ArrayEquals(IEnumerable<string> array, IEnumerable<string> array2)
        {
            if (array is null && array2 is null)
            {
                return true;
            }
            if (array is null || array2 is null)
            {
                return false;
            }
            if (array.Count() != array2.Count())
            {
                return false;
            }

            var list1 = array.ToArray();
            var list2 = array2.ToArray();
            for (var i = 0; i < list1.Length; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }
            return true;
        }
        
        private void MessageEquals(IMessage message, IMessage message2)
        {
            Assert.Equal(message.Id, message2.Id);
            Assert.Equal(message.Method, message2.Method);
            Assert.True(ArrayEquals(message.Params, message2.Params));
            Assert.Equal(message.Ptype, message2.Ptype);
            Assert.Equal(message.Sec, message2.Sec);
            Assert.Equal(message.FieldIndex, message2.FieldIndex);
        }
    }
}

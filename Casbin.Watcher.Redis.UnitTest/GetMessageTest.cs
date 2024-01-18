using System.Threading;
using Casbin.Model;
using Casbin.Watcher.Redis.Entities;
using FluentAssertions;
using Xunit;

namespace Casbin.Watcher.Redis.UnitTest;

public class GetMessageTest
{
    private const int WaitTime = 500;

    [Fact]
    public void TestUpdate()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        watcher.Update();
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.SavePolicy
        });
        watcher.Close();
    }

    [Fact]
    public void TestWatcherWithIgnoreSelfTrue()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher, new WatcherOptions { IgnoreSelf = true });
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        watcher.Update();
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForAddPolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.AddPolicy("alice", "book1", "write");
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.AddPolicy,
            PolicyType = "p",
            Section = "p",
            Values = Policy.ValuesFrom(new[] { "alice", "book1", "write" })
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForAddExistentPolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.AddPolicy("alice", "data1", "read");
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemovePolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemovePolicy("alice", "data1", "read");
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.RemovePolicy,
            PolicyType = "p",
            Section = "p",
            Values = Policy.ValuesFrom(new[] { "alice", "data1", "read" })
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemoveNonExistentPolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemovePolicy("alice", "data4", "read");
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemoveFilteredPolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemoveFilteredPolicy(1, "data1", "read");
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.RemoveFilteredPolicy,
            FieldIndex = 1,
            PolicyType = "p",
            Section = "p",
            ValuesList = Policy.ValuesListFrom(new[] { new[] { "alice", "data1", "read" } })
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemoveFilteredNonExistentPolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemoveFilteredPolicy(1, "data3", "read");
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    [Fact]
    public void TestUpdateSavePolicy()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.SavePolicy();
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.SavePolicy
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForAddPolicies()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.AddPolicies(new[]
        {
                new[]{"jack", "data4", "read"},
                new[]{"katy", "data4", "write"},
                new[]{"leyo", "data4", "read"},
                new[]{"ham", "data4", "write"},
            });
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.AddPolicies,
            PolicyType = "p",
            Section = "p",
            ValuesList = Policy.ValuesListFrom(new[]
            {
                    new[]{"jack", "data4", "read"},
                    new[]{"katy", "data4", "write"},
                    new[]{"leyo", "data4", "read"},
                    new[]{"ham", "data4", "write"}
                })
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForAddAnyExistentPolicies()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.AddPolicies(new[]
        {
                new[]{"data2_admin", "data3", "read"},
                new[]{"alice", "data1", "read"},  // Existent
            });
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemovePolicies()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemovePolicies(new[]
        {
                new[]{"jack", "data4", "read"},
                new[]{"katy", "data4", "write"},
                new[]{"data2_admin", "data2", "write"},
                new[]{"ham", "data4", "write"},
            });
        Thread.Sleep(WaitTime);

        MessageEquals(message, new Message
        {
            Id = watcher.Id,
            Operation = PolicyOperation.RemovePolicies,
            PolicyType = "p",
            Section = "p",
            ValuesList = Policy.ValuesListFrom(new[]
            {
                    new[]{"jack", "data4", "read"},
                    new[]{"katy", "data4", "write"},
                    new[]{"data2_admin", "data2", "write"},
                    new[]{"ham", "data4", "write"}
                })
        });
        watcher.Close();
    }

    [Fact]
    public void TestUpdateForRemoveNonExistentPolicies()
    {
        Message message = null;
        IEnforcer enforcer = InitWatcher(out RedisWatcher watcher);
        watcher.SetUpdateCallback(msg => message = (Message)msg);

        enforcer.RemovePolicies(new[]
        {
                new[]{"jack", "data4", "read"},
                new[]{"katy", "data4", "write"},
                new[]{"leyo", "data4", "read"},
                new[]{"ham", "data4", "write"},
            });
        Thread.Sleep(WaitTime);

        message.Should().BeNull();
        watcher.Close();
    }

    private static IEnforcer InitWatcher(out RedisWatcher watcher, WatcherOptions options = null)
    {
        // For the tests we can't ignore our own messages.
        options = options == null ? new WatcherOptions { IgnoreSelf = false } : options;

        watcher = new RedisWatcher(options: options);
        var enforcer = new Enforcer("examples/rbac_model.conf", "examples/rbac_policy.csv");
        enforcer.SetWatcher(watcher);
        return enforcer;
    }

    private void MessageEquals(Message message, Message message2)
    {
        // Ignore the serialization properties.
        message.Should().BeEquivalentTo(message2, opt => opt
            .Excluding(f => f.SerializableValues)
            .Excluding(f => f.SerializableNewValues)
            .Excluding(f => f.SerializableValuesList)
            .Excluding(f => f.SerializableNewValuesList));
    }
}
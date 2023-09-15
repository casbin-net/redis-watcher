# redis-watcher

[![Actions Status](https://github.com/casbin-net/redis-watcher/workflows/Build/badge.svg)](https://github.com/casbin-net/redis-watcher/actions)
[![Coverage Status](https://coveralls.io/repos/github/casbin-net/redis-watcher/badge.svg?branch=master)](https://coveralls.io/github/casbin-net/redis-watcher?branch=master)
[![License](https://img.shields.io/github/license/casbin-net/redis-watcher)](https://github.com/casbin-net/redis-watcher/blob/master/LICENSE)
[![Build Version](https://img.shields.io/casbin-net.myget/casbin-net/v/Casbin.Watcher.Redis?label=Casbin.Watcher.Redis)](https://www.myget.org/feed/casbin-net/package/nuget/Casbin.Watcher.Redis)

Redis watcher for Casbin.NET

## Installation

```
dotnet add package Casbin.Watcher.Redis
```

## Simple Example

```csharp
using Casbin;
using Redis.Casbin.NET;

public class Program
{
    public static void Main(string[] args)
    {
        // Initialize the watcher.
        // Use the Redis host as parameter.
        var watcher = new RedisWatcher("127.0.0.1:6379");

        // Initialize the enforcer.
        var enforcer = new Enforcer("examples/rbac_model.conf", "examples/rbac_policy.csv");

        // Set the watcher for the enforcer.
        enforcer.SetWatcher(watcher);

        // Update the policy to test the effect.
        enforcer.SavePolicy();
    }
}
```

## Getting Help

- [Casbin.NET](https://github.com/casbin/Casbin.NET)

## License

This project is under Apache 2.0 License. See the [LICENSE](LICENSE) file for the full license text.

using Cflashsoft.Framework.Optimization;
using Cflashsoft.Framework.Redis;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Medallion.Threading.SqlServer;
using StackExchange.Redis;

namespace HybridCacheIntegrationTests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting sanity checks");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");

            var initialKey = "InitialTestKey";
            var initialValue = "Testing 123";

            await redis.GetDatabase().KeyDeleteAsync(initialKey);

            if ((string)await redis.GetDatabase().StringGetAsync(initialKey) != null)
                throw new InvalidOperationException();

            await redis.GetDatabase().StringSetAsync(initialKey, initialValue);

            if ((string)await redis.GetDatabase().StringGetAsync(initialKey) != initialValue)
                throw new InvalidOperationException();

            Console.WriteLine("Sanity checks done.");
            Console.WriteLine("Initializing HybridCache");

            var distributedLockProvider = new RedisDistributedSynchronizationProvider(redis.GetDatabase());
            //var distributedLockProvider = new SqlDistributedSynchronizationProvider("Data Source=.\\SQLEXPRESS;initial catalog=RiverFront;trusted_connection=true;TrustServerCertificate=True;");
            var hybridCacheDistributedLockProvider = new RedisHybridCacheDistributedLockProvider(distributedLockProvider);
            //IHybridCacheDistributedLockProvider hybridCacheDistributedLockProvider = null;
            var hybridCacheProvider = new RedisHybridCacheProvider(redis);
            var changeMonitorProvider = new RedisChangeMonitorProvider(redis);
            var hybridCache = new HybridCache(hybridCacheProvider, hybridCacheDistributedLockProvider, changeMonitorProvider, "DefaultHybridCache", true, true, 3, 20, true);

            Console.WriteLine("Starting HybridCache Tests, checking initial InterlockedGetOrSet works");

            var key = "TestKey";
            var value = new MyClass { MyProperty1 = "Property 1", MyProperty2 = "Property 2!" };

            await redis.GetDatabase().KeyDeleteAsync(key);

            if ((string)await redis.GetDatabase().StringGetAsync(key) != null)
                throw new InvalidOperationException();

            var result = await hybridCache.InterlockedGetOrSetAsync(key, async () => { return value; });

            if (!value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: true, useRemoteCache: false)))
                throw new InvalidOperationException();

            if (!value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: false, useRemoteCache: true)))
                throw new InvalidOperationException();

            Console.WriteLine("Initial check passed, waiting 5 seconds");

            Thread.Sleep(5000);

            Console.WriteLine("2nd tests, checking memorycache item has expired and re-inserting");

            if (value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: true, useRemoteCache: false)))
                throw new InvalidOperationException();

            if (!value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: false, useRemoteCache: true)))
                throw new InvalidOperationException();

            result = await hybridCache.InterlockedGetOrSetAsync(key, async () => { return value; });

            if (!value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: true, useRemoteCache: false)))
                throw new InvalidOperationException();

            if (!value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: false, useRemoteCache: true)))
                throw new InvalidOperationException();

            Console.WriteLine("2nd check passed, waiting 25 seconds");

            Thread.Sleep(25000);

            Console.WriteLine("3rd tests, checking remote cache item has expired and been removed");

            if (value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: true, useRemoteCache: false)))
                throw new InvalidOperationException();

            if (value.Equals(await hybridCache.GetAsync<MyClass>(key, useMemoryCache: false, useRemoteCache: true)))
                throw new InvalidOperationException();

            Console.WriteLine("All tests passed. Done");
        }
    }
}

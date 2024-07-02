using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization.Tests
{
    internal class ObjectCacheExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SyncedGetOrSet_Returns_Object()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            int runCounter = 0;
            var result = cache.InterlockedGetOrSet("test", () =>
            {
                runCounter++;
                return new object();
            });

            // Assert
            runCounter.Should().Be(1);
            result.Should().NotBeNull();
        }

        [Test]
        public void SyncedGetOrSet_Returns_ValueType()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            int runCounter = 0;
            var result = cache.InterlockedGetOrSet("test", () =>
            {
                runCounter++;
                return new (int Field1, string Field2)?((100, "SomeValue"));
            });

            // Assert
            runCounter.Should().Be(1);
            result.Should().NotBeNull();
            result.Value.Field1.Should().Be(100);
            result.Value.Field2.Should().Be("SomeValue");
        }

        [Test]
        public void SyncedGetOrSet_Returns_CachedObject()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            cache.Set("test", new object(), new CacheItemPolicy());

            // Act
            int runCounter = 0;
            var result = cache.InterlockedGetOrSet("test", () =>
            {
                runCounter++;
                return new object();
            });

            // Assert
            runCounter.Should().Be(0);
            result.Should().NotBeNull();
        }

        [Test]
        public void SyncedGetOrSet_Returns_CachedValueType()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            cache.Set("test", new (int Field1, string Field2)?((100, "SomeValue")), new CacheItemPolicy());

            // Act
            int runCounter = 0;
            var result = cache.InterlockedGetOrSet("test", () =>
            {
                runCounter++;
                return new (int Field1, string Field2)?((100, "SomeValue"));
            });

            // Assert
            runCounter.Should().Be(0);
            result.Should().NotBeNull();
            result.Value.Field1.Should().Be(100);
            result.Value.Field2.Should().Be("SomeValue");
        }

        [Test]
        public void SyncedGetOrSet_Exception_WhenGetObjectNull()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            var act = () => cache.InterlockedGetOrSet("test", () => { return (object)null; });

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SyncedGetOrSet_Concurrency()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            int maxCount = 10;
            var tasks = new List<Task<object>>();

            // Act
            int testRunCounter = 0;
            int otherRunCounter = 0;

            for (int count = 0; count < maxCount; count++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var result = cache.InterlockedGetOrSet("test", () =>
                    {
                        Interlocked.Increment(ref testRunCounter);
                        return new object();
                    });

                    return result;
                }));
            }

            for (int count = 0; count < maxCount; count++)
            {
                var key = $"other{count + 1}";
                tasks.Add(Task.Run(() =>
                {
                    var result = cache.InterlockedGetOrSet(key, () =>
                    {
                        Interlocked.Increment(ref otherRunCounter);
                        return new object();
                    });

                    return result;
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            testRunCounter.Should().Be(1);
            otherRunCounter.Should().Be(10);

            for (int count = 0; count < (maxCount * 2); count++)
            {
                tasks[count].Result.Should().NotBeNull();
            }
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Returns_Object()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            int runCounter = 0;
            var result = await cache.InterlockedGetOrSetAsync("test", async () =>
            {
                runCounter++;
                return new object();
            });

            // Assert
            runCounter.Should().Be(1);
            result.Should().NotBeNull();
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Returns_ValueType()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            int runCounter = 0;
            var result = await cache.InterlockedGetOrSetAsync("test", async () =>
            {
                runCounter++;
                return new (int Field1, string Field2)?((100, "SomeValue"));
            });

            // Assert
            runCounter.Should().Be(1);
            result.Should().NotBeNull();
            result.Value.Field1.Should().Be(100);
            result.Value.Field2.Should().Be("SomeValue");
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Returns_CachedObject()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            cache.Set("test", new object(), new CacheItemPolicy());

            // Act
            int runCounter = 0;
            var result = await cache.InterlockedGetOrSetAsync("test", async () =>
            {
                runCounter++;
                return new object();
            });

            // Assert
            runCounter.Should().Be(0);
            result.Should().NotBeNull();
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Returns_CachedValueType()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            cache.Set("test", new (int Field1, string Field2)?((100, "SomeValue")), new CacheItemPolicy());

            // Act
            int runCounter = 0;
            var result = await cache.InterlockedGetOrSetAsync("test", async () =>
            {
                runCounter++;
                return new (int Field1, string Field2)?((100, "SomeValue"));
            });

            // Assert
            runCounter.Should().Be(0);
            result.Should().NotBeNull();
            result.Value.Field1.Should().Be(100);
            result.Value.Field2.Should().Be("SomeValue");
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Concurrency()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");
            int maxCount = 10;
            var tasks = new List<Task<object>>();

            // Act
            int testRunCounter = 0;
            int otherRunCounter = 0;

            for (int count = 0; count < maxCount; count++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var result = await cache.InterlockedGetOrSetAsync("test", async () =>
                    {
                        Interlocked.Increment(ref testRunCounter);
                        return new object();
                    });

                    return result;
                }));
            }

            for (int count = 0; count < maxCount; count++)
            {
                var key = $"other{count + 1}";
                tasks.Add(Task.Run(async () =>
                {
                    var result = await cache.InterlockedGetOrSetAsync(key, async () =>
                    {
                        Interlocked.Increment(ref otherRunCounter);
                        return new object();
                    });

                    return result;
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            // Assert
            testRunCounter.Should().Be(1);
            otherRunCounter.Should().Be(10);

            for (int count = 0; count < (maxCount * 2); count++)
            {
                tasks[count].Result.Should().NotBeNull();
            }
        }

        [Test]
        public async Task SyncedGetOrSetAsync_Exception_WhenGetObjectNull()
        {
            // Arrange
            using var cache = new MemoryCache("ObjectCacheExtensionsTests");

            // Act
            var act = () => cache.InterlockedGetOrSetAsync("test", async () => { return (object)null; });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
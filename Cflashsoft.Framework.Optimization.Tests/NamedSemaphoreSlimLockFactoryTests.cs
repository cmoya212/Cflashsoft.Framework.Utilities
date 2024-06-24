using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization.Tests
{
    [TestFixture]
    internal class NamedSemaphoreSlimLockFactoryTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CreateLock_With_Null_Name_Throws_ArgumentNullException()
        {
            // Arrange
            var factory = new NamedSemaphoreSlimLockFactory();
            string name = null;

            // Act
            Action act = () => factory.Get(name);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void RemoveLock_With_Null_Name_Throws_ArgumentNullException()
        {
            // Arrange
            var factory = new NamedSemaphoreSlimLockFactory();
            string name = null;

            // Act
            Action act = () => factory.Remove(name);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void RemoveLock_DoesNotExist_DoesNotThrow_Exception()
        {
            // Arrange
            var factory = new NamedSemaphoreSlimLockFactory();
            var name = "NonExistentKey";

            // Act
            Action act = () => factory.Remove(name);

            // Assert
            act.Should().NotThrow<Exception>();
        }

        [Test]
        public void CreateLock_Returns_Lock()
        {
            // Arrange
            var factory = new NamedSemaphoreSlimLockFactory();
            var name = "test";

            // Act
            var semaphore = factory.Get(name);

            // Assert
            semaphore.Should().NotBeNull();
        }

        [Test]
        public void RemoveLock_Removes_Lock()
        {
            // Arrange
            var factory = new NamedSemaphoreSlimLockFactory();
            var name = "test";

            // Act
            var lock1 = factory.Get(name);
            factory.Remove(name);
            var lock2 = factory.Get(name);

            // Assert
            lock1.Should().NotBeSameAs(lock2);
        }
    }
}
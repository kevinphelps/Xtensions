﻿namespace Xtensions.Cache.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class CacheExtensionsTests
    {
        [Fact]
        public async Task Read_EntryDoesNotExist_ReturnsNull()
        {
            const string cacheKey = "test-cache-key";

            Mock<ICache> cacheMock = new Mock<ICache>(MockBehavior.Strict);

            cacheMock
                .Setup(cache => cache.ReadEntry<string>(cacheKey))
                .ReturnsAsync(null as CacheEntry<string>);

            Assert.Null(await cacheMock.Object.Read<string>(cacheKey: cacheKey));
        }

        [Fact]
        public async Task Read_EntryExists_ReturnsValue()
        {
            const string cacheKey = "test-cache-key";
            const string value = "test-value";
            DateTime absoluteExpiration = new DateTime(year: 2018, month: 3, day: 17, hour: 8, minute: 0, second: 0);

            Mock<ICache> cacheMock = new Mock<ICache>(MockBehavior.Strict);

            cacheMock
                .Setup(cache => cache.ReadEntry<string>(cacheKey))
                .ReturnsAsync(new CacheEntry<string>(value, absoluteExpiration));

            string? result = await cacheMock.Object.Read<string>(cacheKey: cacheKey);

            Assert.Equal(actual: result, expected: value);
        }

        [Fact]
        public async Task Write_WritesValue()
        {
            const string cacheKey = "test-cache-key";
            const string value = "test-value";
            DateTime absoluteExpiration = new DateTime(year: 2018, month: 3, day: 17, hour: 8, minute: 0, second: 0);

            Mock<ICache> cacheMock = new Mock<ICache>(MockBehavior.Strict);

            cacheMock
                .Setup(cache => cache.WriteEntry(
                    cacheKey,
                    It.Is<CacheEntry<string>>(cacheEntry => cacheEntry.Value == value && cacheEntry.AbsoluteExpiration == absoluteExpiration)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await cacheMock.Object.Write(cacheKey: cacheKey, value: value, absoluteExpiration: absoluteExpiration);

            cacheMock.Verify();
        }

        [Fact]
        public async Task WriteEntry_WritesEntryToAllCaches()
        {
            const string cacheKey = "test-cache-key";
            const string value = "test-value";
            DateTime absoluteExpiration = new DateTime(year: 2018, month: 3, day: 17, hour: 8, minute: 0, second: 0);
            CacheEntry<string> cacheEntry = new CacheEntry<string>(value, absoluteExpiration);

            Mock<ICache> cache1Mock = new Mock<ICache>(MockBehavior.Strict);

            cache1Mock
                .Setup(cache => cache.WriteEntry(cacheKey, cacheEntry))
                .Returns(Task.CompletedTask).Verifiable();

            Mock<ICache> cache2Mock = new Mock<ICache>(MockBehavior.Strict);

            cache2Mock
                .Setup(cache => cache.WriteEntry(cacheKey, cacheEntry))
                .Returns(Task.CompletedTask).Verifiable();

            IReadOnlyCollection<ICache> caches = new ICache[] { cache1Mock.Object, cache2Mock.Object };

            await caches.WriteEntry(cacheKey, cacheEntry);

            cache1Mock.Verify();
            cache2Mock.Verify();
        }
    }
}

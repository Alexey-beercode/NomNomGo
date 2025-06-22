// CacheServiceTests.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NomNomGo.IdentityService.Infrastructure.Services;

namespace NomNomGo.IdentityService.Tests;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _cacheService = new CacheService(_mockCache.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedData()
    {
        var key = "testKey";
        var testData = new { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testData);
        var bytes = Encoding.UTF8.GetBytes(json);

        _mockCache.Setup(c => c.GetAsync(key, default)).ReturnsAsync(bytes);

        var result = await _cacheService.GetAsync<object>(key);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault()
    {
        var key = "missingKey";
        _mockCache.Setup(c => c.GetAsync(key, default)).ReturnsAsync((byte[]?)null);

        var result = await _cacheService.GetAsync<object>(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_SetsValueInCache()
    {
        var key = "testKey";
        var value = new { Id = 1, Name = "Test" };
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        _mockCache.Setup(c => c.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                  .Returns(Task.CompletedTask);

        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(10));

        _mockCache.Verify(c => c.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_RemovesKeyFromCache()
    {
        var key = "testKey";
        _mockCache.Setup(c => c.RemoveAsync(key, default)).Returns(Task.CompletedTask);

        await _cacheService.RemoveAsync(key);

        _mockCache.Verify(c => c.RemoveAsync(key, default), Times.Once);
    }
}

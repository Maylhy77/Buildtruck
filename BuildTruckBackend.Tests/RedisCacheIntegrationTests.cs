using BuildTruckMaterialsService.Materials.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BuildTruckBackend.Tests;

/// <summary>
/// Verifica RedisMaterialCacheService contra un Redis real.
/// Se salta si REDIS_TEST_CONNECTION no esta definido.
/// </summary>
public class RedisCacheIntegrationTests
{
    private static string? Connection => Environment.GetEnvironmentVariable("REDIS_TEST_CONNECTION");

    private static RedisMaterialCacheService CreateSut(IDistributedCache cache)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["CacheSettings:TtlSeconds"] = "300" })
            .Build();

        return new RedisMaterialCacheService(
            cache,
            NullLogger<RedisMaterialCacheService>.Instance,
            config);
    }

    private static IDistributedCache RealRedis() =>
        new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = Connection,
            InstanceName = "buildtruck-test:"
        }));

    [SkippableFact]
    public async Task CachesProjectList_AndHitsCacheOnSecondCall()
    {
        Skip.If(string.IsNullOrEmpty(Connection), "REDIS_TEST_CONNECTION no definido");
        var sut = CreateSut(RealRedis());

        var dbCalls = 0;
        Task<List<string>?> Factory()
        {
            dbCalls++;
            return Task.FromResult<List<string>?>(new List<string> { "cemento", "arena" });
        }

        var projectId = Random.Shared.Next(100000, 999999);

        var first = await sut.GetOrSetProjectAsync(projectId, "list", Factory);
        var second = await sut.GetOrSetProjectAsync(projectId, "list", Factory);

        Assert.Equal(new[] { "cemento", "arena" }, first!);
        Assert.Equal(new[] { "cemento", "arena" }, second!);
        Assert.Equal(1, dbCalls); // la segunda salio del cache
    }

    [SkippableFact]
    public async Task InvalidateProject_ForcesRefetch()
    {
        Skip.If(string.IsNullOrEmpty(Connection), "REDIS_TEST_CONNECTION no definido");
        var sut = CreateSut(RealRedis());

        var dbCalls = 0;
        var projectId = Random.Shared.Next(100000, 999999);
        Task<List<string>?> Factory()
        {
            dbCalls++;
            return Task.FromResult<List<string>?>(new List<string> { $"v{dbCalls}" });
        }

        await sut.GetOrSetProjectAsync(projectId, "list", Factory);
        await sut.GetOrSetProjectAsync(projectId, "list", Factory);
        Assert.Equal(1, dbCalls);

        await sut.InvalidateProjectAsync(projectId);

        var after = await sut.GetOrSetProjectAsync(projectId, "list", Factory);
        Assert.Equal(2, dbCalls);            // volvio a la BD
        Assert.Equal(new[] { "v2" }, after!); // y devolvio el dato nuevo
    }

    [Fact]
    public async Task WhenCacheIsDown_FallsBackToFactory()
    {
        // Puerto cerrado a proposito: simula Redis caido.
        var broken = new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = "127.0.0.1:6390,connectTimeout=200,abortConnect=false"
        }));
        var sut = CreateSut(broken);

        var result = await sut.GetOrSetProjectAsync(
            1, "list", () => Task.FromResult<List<string>?>(new List<string> { "desde-bd" }));

        Assert.Equal(new[] { "desde-bd" }, result!);
    }
}

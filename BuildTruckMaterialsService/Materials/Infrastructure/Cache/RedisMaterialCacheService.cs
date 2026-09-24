using System.Text.Json;
using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace BuildTruckMaterialsService.Materials.Infrastructure.Cache;

/// <summary>
/// Adaptador de cache sobre IDistributedCache.
///
/// Invalidacion por versionado: en vez de borrar claves con wildcard
/// (KEYS bloquea Redis), cada proyecto tiene un contador de version que
/// forma parte de la clave. Incrementarlo deja las claves viejas huerfanas,
/// y la politica allkeys-lru las descarta cuando hace falta espacio.
/// </summary>
public class RedisMaterialCacheService : IMaterialCacheService
{
    private const string VersionPrefix = "materials:ver:project:";

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisMaterialCacheService> _logger;
    private readonly DistributedCacheEntryOptions _options;

    public RedisMaterialCacheService(
        IDistributedCache cache,
        ILogger<RedisMaterialCacheService> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;

        var ttl = configuration.GetValue("CacheSettings:TtlSeconds", 300);
        _options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl)
        };
    }

    public async Task<T?> GetOrSetProjectAsync<T>(
        int projectId, string suffix, Func<Task<T?>> factory, CancellationToken ct = default)
    {
        var version = await GetProjectVersionAsync(projectId, ct);
        return await GetOrSetAsync($"materials:project:{projectId}:{suffix}:v{version}", factory, ct);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, CancellationToken ct = default)
    {
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            if (cached is not null)
                return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            // Un cache caido nunca debe tumbar una lectura: degradamos a la BD.
            _logger.LogWarning(ex, "Cache no disponible al leer {Key}; consultando origen", key);
            return await factory();
        }

        var value = await factory();
        if (value is null) return default;

        try
        {
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), _options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo escribir en cache la clave {Key}", key);
        }

        return value;
    }

    public Task InvalidateProjectAsync(int projectId, CancellationToken ct = default)
        => BumpVersionAsync($"{VersionPrefix}{projectId}", ct);

    public async Task InvalidateMaterialAsync(int materialId, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync($"materials:material:{materialId}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo invalidar el material {MaterialId}", materialId);
        }
    }

    /// <summary>Version actual de un proyecto; forma parte de sus claves de cache.</summary>
    private async Task<long> GetProjectVersionAsync(int projectId, CancellationToken ct = default)
    {
        try
        {
            var raw = await _cache.GetStringAsync($"{VersionPrefix}{projectId}", ct);
            return raw is not null && long.TryParse(raw, out var v) ? v : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task BumpVersionAsync(string versionKey, CancellationToken ct)
    {
        try
        {
            var raw = await _cache.GetStringAsync(versionKey, ct);
            var next = raw is not null && long.TryParse(raw, out var v) ? v + 1 : 1;
            await _cache.SetStringAsync(versionKey, next.ToString(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo invalidar la version {VersionKey}", versionKey);
        }
    }
}

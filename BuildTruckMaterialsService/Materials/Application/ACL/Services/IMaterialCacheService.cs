namespace BuildTruckMaterialsService.Materials.Application.ACL.Services;

/// <summary>
/// Puerto de cache para el contexto Materials.
/// La implementacion (Redis, memoria, etc.) vive en Infrastructure.
/// </summary>
public interface IMaterialCacheService
{
    /// <summary>
    /// Devuelve lo cacheado para un proyecto, o ejecuta <paramref name="factory"/>
    /// y cachea el resultado. La clave se versiona por proyecto, de modo que
    /// InvalidateProjectAsync deja obsoletas todas sus entradas de golpe.
    /// Si el cache falla, cae a <paramref name="factory"/>.
    /// </summary>
    Task<T?> GetOrSetProjectAsync<T>(int projectId, string suffix, Func<Task<T?>> factory, CancellationToken ct = default);

    /// <summary>Igual que el anterior, pero para una entidad suelta sin version.</summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, CancellationToken ct = default);

    /// <summary>Invalida todas las claves cacheadas de un proyecto.</summary>
    Task InvalidateProjectAsync(int projectId, CancellationToken ct = default);

    /// <summary>Invalida la clave de un material concreto.</summary>
    Task InvalidateMaterialAsync(int materialId, CancellationToken ct = default);
}

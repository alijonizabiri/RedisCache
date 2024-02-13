using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisCacheDemo.Services.CacheServices;

public class CacheService(ILogger<CacheService> logger, IDistributedCache distributedCache)
    : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var dataInCache = await distributedCache.GetStringAsync(key, cancellationToken);

        if (dataInCache != null)
        {
            await Console.Out.WriteLineAsync(new string('*', 120));
            logger.LogInformation("Redis : Data retrieved from cache  key : {key}", key);
            await Console.Out.WriteLineAsync(new string('*', 120));
            
            return JsonSerializer.Deserialize<T>(dataInCache);
        }
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogWarning("Redis :There is no data in the cache for this key : {key}", key);
        await Console.Out.WriteLineAsync(new string('*', 120));
        return default;
    }

    public async Task AddAsync<T>(string key, T entity, DateTimeOffset expirationTime, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonSerializerOption = new JsonSerializerOptions() { WriteIndented = true };
            var jsonObject = JsonSerializer.Serialize(entity, jsonSerializerOption);
            var cacheOption = new DistributedCacheEntryOptions() { AbsoluteExpiration = expirationTime };
            await distributedCache.SetStringAsync(key, jsonObject, cacheOption, cancellationToken);
            
            await Console.Out.WriteLineAsync(new string('*', 120));
            logger.LogInformation("Redis : Added new data to cache by key: {key}, expiration time before : {expirationTime}", key, expirationTime);
            await Console.Out.WriteLineAsync(new string('*', 120));
        }
        catch (Exception ex)
        {
            logger.LogError("Redis error: {0}", ex.Message);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            
            await Console.Out.WriteLineAsync(new string('*', 120));
            logger.LogInformation("Redis : Deleted data from cache by key: {key}", key);
            await Console.Out.WriteLineAsync(new string('*', 120));
        }
        catch (Exception exception)
        {
            logger.LogError("Redis error: {0}", exception.Message);
        }
    }
}

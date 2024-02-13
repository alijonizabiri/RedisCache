namespace RedisCacheDemo.Services.CacheServices;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task AddAsync<T>(string key, T entity, DateTimeOffset expirationTime, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

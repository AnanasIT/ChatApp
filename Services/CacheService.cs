
using Microsoft.Extensions.Caching.Memory;
using ICache;

public class CacheService : ICacheService
{
    public readonly IMemoryCache _cache;
    public readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug($"Кэш HIT: {key}");
            return value;
        }

        _logger.LogDebug($"Кэш MISS: {key}");
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        
        }

        else
        {
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        }

        _cache.Set(key, value, options);
        _logger.LogDebug($"Кэш SET: {key}, время жизни: {options.AbsoluteExpirationRelativeToNow?.TotalSeconds}с");
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug($"Кэш REMOVE: {key}");
    }


    public bool TryGet<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    public void Clear()
    {
        _cache.Dispose();
        _logger.LogInformation("Кэш очищен");
    }
}
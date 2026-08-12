namespace ICache;

public interface ICacheService
{
    public T? Get<T>(string key);
    public void Set<T>(string key, T value, TimeSpan? expiration = null);
    public void Remove(string key);
    public bool TryGet<T>(string key, out T? value);
    public bool Exists(string key);
    public void Clear();
}
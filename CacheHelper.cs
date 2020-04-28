using Microsoft.Extensions.Caching.Memory;
using System;

public class CacheHelper
{
    static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

    /// <summary>
    /// 获取缓存中的值
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    public static object GetCacheValue(string key)
    {
        if (!string.IsNullOrEmpty(key) && Cache.TryGetValue(key, out var val))
        {
            return val;
        }
        return default(object);
    }

    public static T GetCacheValue<T>(string key)
    {
        if (!string.IsNullOrEmpty(key) && Cache.TryGetValue(key, out var val))
        {
            return (T)val;
        }
        return default(T);
    }

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public static void SetCacheValue(string key, object value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            Cache.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            });
        }
    }

    public static void SetCacheValue<T>(string key, T value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            Cache.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            });
        }
    }
}
using System.Collections.Concurrent;
using System.Collections;
using Microsoft.Extensions.Caching.Memory;

namespace IIROSA.Application.Caching;

/// <summary>Shared cache-aside helper with per-key request coalescing and prefix invalidation.</summary>
public sealed class EntityMemoryCache
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.Ordinal);
    private readonly object _mutationLock = new();
    private long _generation;

    public EntityMemoryCache(IMemoryCache cache) => _cache = cache;

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? lifetime = null, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null) return cached;

        var gate = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cache.TryGetValue(key, out cached) && cached is not null) return cached;
            var generation = Interlocked.Read(ref _generation);
            var value = await factory(cancellationToken).ConfigureAwait(false);
            var cacheable = value is not null
                && value is not ICacheResultPolicy { ShouldCache: false }
                && value is not ICollection { Count: 0 };
            if (cacheable)
            {
                lock (_mutationLock)
                {
                    if (generation == _generation)
                    {
                        _keys.TryAdd(key, 0);
                        var options = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = lifetime ?? DefaultLifetime
                        };
                        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                        {
                            EvictionCallback = static (evictedKey, evictedValue, reason, state) =>
                            {
                                if (evictedKey is string cacheKey)
                                    ((ConcurrentDictionary<string, byte>)state!).TryRemove(cacheKey, out var removed);
                            },
                            State = _keys
                        });
                        _cache.Set(key, value, options);
                    }
                }
            }
            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Remove(string key)
    {
        lock (_mutationLock)
        {
            _generation++;
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }

    public void RemovePrefix(string prefix)
    {
        lock (_mutationLock)
        {
            _generation++;
            foreach (var key in _keys.Keys)
            {
                if (!key.StartsWith(prefix, StringComparison.Ordinal)) continue;
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }
        }
    }
}

public interface ICacheResultPolicy
{
    bool ShouldCache { get; }
}

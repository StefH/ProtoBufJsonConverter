using System.Collections.Concurrent;

namespace ProtoBufJsonConverter.Extensions;

internal static class DictionaryExtensions
{
    internal static async Task<TValue> GetOrAddAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, Lazy<Task<TValue>>> dictionary,
        TKey key,
        Func<TKey, Task<TValue>> valueFactory)
        where TKey : notnull
    {
        var lazyTask = dictionary.GetOrAdd(
            key,
            k => new Lazy<Task<TValue>>(() => valueFactory(k), isThreadSafe: true));

        try
        {
            return await lazyTask.Value.ConfigureAwait(false);
        }
        catch
        {
            // Remove failed task so next call can retry
            dictionary.TryRemove(key, out _);
            throw;
        }
    }
}
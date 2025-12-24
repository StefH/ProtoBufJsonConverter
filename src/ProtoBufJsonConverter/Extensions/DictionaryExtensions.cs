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
            // Use ICollection.Remove for value comparison to ensure we only remove the specific failed lazyTask instance
            // This prevents race condition where another thread might have already added a new task
            // Note: ConcurrentDictionary.TryRemove(KeyValuePair) is only available in .NET 5.0+,
            // but we need to support net462, net48, and netstandard2.1
            ((ICollection<KeyValuePair<TKey, Lazy<Task<TValue>>>>)dictionary).Remove(new KeyValuePair<TKey, Lazy<Task<TValue>>>(key, lazyTask));
            throw;
        }
    }
}
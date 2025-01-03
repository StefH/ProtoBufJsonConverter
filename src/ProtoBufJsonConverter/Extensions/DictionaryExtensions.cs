using Stef.Validation;

namespace ProtoBufJsonConverter.Extensions;

internal static class DictionaryExtensions
{
    internal static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> valueFactory)
    {
        Guard.NotNull(dictionary);
        Guard.NotNull(key);

        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        value = await valueFactory(key);
        dictionary.Add(key, value);

        return value;
    }
}
namespace ProtoBufJsonConverter.Utils;

internal static class TypeUtils
{
    internal static T ChangeType<T>(object? value, T defaultValue) where T : notnull
    {
        if (value == null)
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
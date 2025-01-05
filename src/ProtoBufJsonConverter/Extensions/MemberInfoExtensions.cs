using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ProtoBufJsonConverter.Extensions;

internal static class MemberInfoExtensions
{
    internal static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, [NotNullWhen(true)] out T? attribute)
        where T : Attribute
    {
        try
        {
            attribute = memberInfo.GetCustomAttribute<T>();
            return attribute != null;
        }
        catch
        {
            attribute = null;
            return false;
        }
    }
}
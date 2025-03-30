using System.Collections.Generic;
using System.Linq;

namespace BlazorWith3d.CodeGenerator;
internal static class StringUtilities
{
    public static string ToCamelCase(this string text)
    {
        return text.Substring(0, 1).ToLowerInvariant() + text.Substring(1);
    }
    public static string JoinStringWithComma<T>(this IEnumerable<T> items)
    {
        return string.Join(", ", items);
    }
    public static string WrapWithParenthesis(this string text, bool shouldWrap)
    {
        if (shouldWrap)
        {
            return $"({text})";
        }
        return text;
    }
    public static IEnumerable<T> ConcatOptional<T>(this IEnumerable<T> items, IEnumerable<T> itemsToAdd, bool shouldConcat)
    {
        if (shouldConcat)
        {
            return items.Concat(itemsToAdd);
        }
        return items;
    }
    public static IEnumerable<T> ConcatOptional<T>(this IEnumerable<T> items, T itemToAdd, bool shouldConcat)
    {
        if (shouldConcat)
        {
            return items.Concat([itemToAdd]);
        }
        return items;
    }
}

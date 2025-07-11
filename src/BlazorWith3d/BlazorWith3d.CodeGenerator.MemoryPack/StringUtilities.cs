using System.Collections.Generic;
using System.Linq;

namespace BlazorWith3d.CodeGenerator;
internal static class StringUtilities
{
    public static string JoinStringWithComma<T>(this IEnumerable<T> items)
    {
        return string.Join(", ", items);
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

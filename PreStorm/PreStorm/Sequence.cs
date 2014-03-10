using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    internal static class Sequence
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items, int size)
        {
            return items
                .Select((item, i) => new { group = i / size, item })
                .GroupBy(o => o.group, o => o.item);
        }
    }
}

using System.Collections.Generic;

namespace Plugins.Extensions
{
    public static class ReadOnlyListExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Equals(list[i], item))
                    return i;
            }

            return -1;
        }
    }
}
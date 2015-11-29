using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public static class LINQ
    {
        public static int Count<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            int count = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    count++;
            }

            return count;
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource target)
        {
            foreach (var item in source)
            {
                if (item.Equals(target))
                    return true;
            }

            return false;
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                    return item;
            }

            return default(TSource);
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> getter)
        {
            int sum = 0;

            foreach (var item in source)
            {
                sum += getter(item);
            }

            return sum;
        }

        public static IEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getter)
            where TKey : IComparable<TKey>
        {
            var items = new List<TSource>();

            foreach (var i in source)
            {
                items.Add(i);
            }

            items.Sort(new DelegateComparer<TSource, TKey>(getter));

            return items;
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            if (source is TSource[])
                return source as TSource[];

            var items = new List<TSource>();

            foreach (var i in source)
            {
                items.Add(i);
            }

            return items.ToArray();
        }
    }

    class DelegateComparer<TSource, TKey> : IComparer<TSource>
        where TKey : IComparable<TKey>
    {
        private Func<TSource, TKey> m_Getter;

        public DelegateComparer(Func<TSource, TKey> getter)
        {
            m_Getter = getter;
        }

        public int Compare(TSource x, TSource y)
        {
            return m_Getter(x).CompareTo(m_Getter(y));
        }
    }
}

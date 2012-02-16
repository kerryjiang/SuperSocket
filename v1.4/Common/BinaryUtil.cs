using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SuperSocket.Common
{
    public static class BinaryUtil
    {
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
            where T : IEquatable<T>
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }

        public static int? SearchMark<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return SearchMark(source, 0, source.Count, mark, 0);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            return SearchMark(source, offset, length, mark, 0);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched)
            where T : IEquatable<T>
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = matched;

            if (matched > 0)
            {
                for (int i = matchCount; i < mark.Length; i++)
                {
                    if (!source[pos++].Equals(mark[i]))
                        break;

                    matchCount++;

                    if (pos > endOffset)
                    {
                        if (matchCount == mark.Length)
                            return offset;
                        else
                            return (0 - matchCount);
                    }
                }

                if (matchCount == mark.Length)
                    return offset;

                pos = offset;
                matchCount = 0;
            }

            while (true)
            {
                pos = source.IndexOf(mark[matchCount], pos, length - pos + offset);

                if (pos < 0)
                    return null;

                matchCount += 1;

                for (int i = matchCount; i < mark.Length; i++)
                {
                    int checkPos = pos + i;

                    if (checkPos > endOffset)
                    {
                        //found end, return matched chars count
                        return (0 - matchCount);
                    }

                    if (!source[checkPos].Equals(mark[i]))
                        break;

                    matchCount++;
                }

                if (matchCount == mark.Length)
                    return pos;

                //Reset next round read pos
                pos += 1;
                //clear matched chars count
                matchCount = 0;
            }
        }

        public static int SearchMark<T>(this IList<T> source, int offset, int length, SearchMarkState<T> searchState)
            where T : IEquatable<T>
        {
            int? result = source.SearchMark(offset, length, searchState.Mark, searchState.Matched);

            if (!result.HasValue)
            {
                searchState.Matched = 0;
                return -1;
            }

            if (result.Value < 0)
            {
                searchState.Matched = 0 - result.Value;
                return -1;
            }

            searchState.Matched = 0;
            return result.Value;
        }

        public static int StartsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.StartsWith(0, source.Count, mark);
        }

        public static int StartsWith<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {            
            int pos = offset;
            int endOffset = offset + length - 1;

            for (int i = 0; i < mark.Length; i++)
            {
                int checkPos = pos + i;

                if (checkPos > endOffset)
                    return i;

                if (!source[checkPos].Equals(mark[i]))
                    return -1;
            }

            return mark.Length;
        }

        public static bool EndsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.EndsWith(0, source.Count, mark);
        }

        public static bool EndsWith<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            if (mark.Length > length)
                return false;

            for (int i = 0; i < Math.Min(length, mark.Length); i++)
            {
                if (!mark[i].Equals(source[offset + length - mark.Length + i]))
                    return false;
            }

            return true;
        }

        public static T[] CloneRange<T>(this T[] source, int offset, int length)
        {
            T[] target = new T[length];
            Array.Copy(source, offset, target, 0, length);
            return target;
        }
    }
}

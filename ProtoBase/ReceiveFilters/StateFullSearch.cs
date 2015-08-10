using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Statefull search util class
    /// </summary>
    public static class StateFullSearch
    {
        /// <summary>
        /// Search target from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="parsedLength">Length of the parsed.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, T[] mark, out int parsedLength)
            where T : IEquatable<T>
        {
            return SearchMark(source, 0, source.Count, mark, 0, out parsedLength);
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            int parsedLength;
            return SearchMark(source, 0, source.Count, mark, 0, out parsedLength);
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            int parsedLength;
            return SearchMark(source, offset, length, mark, 0, out parsedLength);
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="parsedLength">Length of the parsed.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, out int parsedLength)
            where T : IEquatable<T>
        {
            return SearchMark(source, offset, length, mark, 0, out parsedLength);
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="matched">The matched.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched)
            where T : IEquatable<T>
        {
            int parsedLength;
            return source.SearchMark(offset, length, mark, matched, out parsedLength);
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <param name="matched">The matched.</param>
        /// <param name="parsedLength">Length of the parsed.</param>
        /// <returns></returns>
        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched, out int parsedLength)
            where T : IEquatable<T>
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = matched;
            parsedLength = 0;

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
                        {
                            parsedLength = mark.Length - matched;
                            return offset;
                        }
                        else
                        {
                            return (0 - matchCount);
                        }
                    }
                }

                if (matchCount == mark.Length)
                {
                    parsedLength = mark.Length - matched;
                    return offset;
                }

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

                //found the full end mark
                if (matchCount == mark.Length)
                {
                    parsedLength = pos - offset + mark.Length;
                    return pos;
                }

                //Reset next round read pos
                pos += 1;
                //clear matched chars count
                matchCount = 0;
            }
        }

        /// <summary>
        /// Searches the mark from source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="searchState">State of the search.</param>
        /// <param name="parsedLength">Length of the parsed.</param>
        /// <returns></returns>
        public static int SearchMark<T>(this IList<T> source, int offset, int length, SearchMarkState<T> searchState, out int parsedLength)
            where T : IEquatable<T>
        {
            int? result = source.SearchMark(offset, length, searchState.Mark, searchState.Matched, out parsedLength);

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

        /// <summary>
        /// Startses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static int StartsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.StartsWith(0, source.Count, mark);
        }

        /// <summary>
        /// Startses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Endses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static bool EndsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.EndsWith(0, source.Count, mark);
        }

        /// <summary>
        /// Endses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Clones the elements in the specific range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static T[] CloneRange<T>(this IList<T> source, int offset, int length)
        {
            T[] target;

            var array = source as T[];

            if (array != null)
            {
                target = new T[length];
                Array.Copy(array, offset, target, 0, length);
                return target;
            }

            target = new T[length];

            for (int i = 0; i < length; i++)
            {
                target[i] = source[offset + i];
            }

            return target;
        }
    }
}

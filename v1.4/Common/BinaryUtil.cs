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
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }

        public static int? SearchMark<T>(this IList<T> source, T[] mark)
        {
            return SearchMark(source, 0, source.Count, mark, 0);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            return SearchMark(source, offset, length, mark, 0);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched)
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = matched;

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

        public static int StartsWith<T>(this IList<T> source, T[] mark)
        {
            return source.StartsWith(0, source.Count, mark);
        }

        public static int StartsWith<T>(this IList<T> source, int offset, int length, T[] mark)
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
        {
            return source.EndsWith(0, source.Count, mark);
        }

        public static bool EndsWith<T>(this IList<T> source, int offset, int length, T[] mark)
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

        public static int GetTotalCount(this List<ArraySegment<byte>> arraySegments)
        {
            if (arraySegments == null || arraySegments.Count <= 0)
                return 0;

            int total = 0;

            for (int i = 0; i < arraySegments.Count; i++)
            {
                total += arraySegments[i].Count;
            }

            return total;
        }

        public static string Decode(this List<ArraySegment<byte>> arraySegments, Encoding encoding)
        {
            if (arraySegments == null || arraySegments.Count <= 0)
                return string.Empty;

            int total = 0;

            for (int i = 0; i < arraySegments.Count; i++)
            {
                total += arraySegments[i].Count;
            }

            var charsBuffer = new char[encoding.GetMaxCharCount(arraySegments.GetTotalCount())];

            int bytesUsed, charsUsed;
            bool completed;
            int totalChars = 0;

            int lastSegIndex = arraySegments.Count - 1;
            var flush = false;

            var decoder = encoding.GetDecoder();

            for (var i = 0; i < arraySegments.Count; i++)
            {
                var segment = arraySegments[i];

                if (i == lastSegIndex)
                    flush = true;

                decoder.Convert(segment.Array, segment.Offset, segment.Count, charsBuffer, totalChars, charsBuffer.Length - totalChars, flush, out bytesUsed, out charsUsed, out completed);
                totalChars += charsUsed;
            }

            return new string(charsBuffer, 0, totalChars);
       }
    }
}

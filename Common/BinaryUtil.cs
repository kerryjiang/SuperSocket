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

        public static int? SearchMark(IList<byte> source, byte[] mark)
        {
            return SearchMark(source, 0, source.Count, mark);
        }  

        public static int? SearchMark(IList<byte> source, int offset, int length, byte[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = 0;

            while (true)
            {
                pos = source.IndexOf(mark[0], pos, length - pos + offset);

                if (pos < 0)
                    return null;

                matchCount = 1;

                for (int i = 1; i < mark.Length; i++)
                {
                    int checkPos = pos + i;

                    if (checkPos > endOffset)
                    {
                        //found end, return matched chars count
                        return (0 - i);
                    }

                    if (source[checkPos] != mark[i])
                        break;

                    matchCount++;
                }

                if (matchCount == mark.Length)
                    return pos;

                //Reset next round read pos
                pos += matchCount;
                //clear matched chars count
                matchCount = 0;
            }
        }

        public static int StartWith(byte[] source, byte[] mark)
        {
            return StartWith(source, 0, source.Length, mark);
        }

        public static int StartWith(byte[] source, int offset, int length, byte[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;

            for (int i = 0; i < mark.Length; i++)
            {
                int checkPos = pos + i;

                if (checkPos > endOffset)
                    return i;

                if (source[checkPos] != mark[i])
                    return -1;
            }

            return mark.Length;
        }
    }
}

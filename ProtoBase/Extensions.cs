using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public static class Extensions
    {
        public static string GetString(this Encoding encoding, IList<ArraySegment<byte>> data)
        {
            var total = data.Sum(x => x.Count);

            var output = new char[encoding.GetMaxCharCount(total)];

            var decoder = encoding.GetDecoder();

            var totalCharsLen = 0;
            var lastIndex = data.Count - 1;
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;

            for (var i = 0; i < data.Count; i++)
            {
                var segment = data[i];

                decoder.Convert(segment.Array, segment.Offset, segment.Count, output, totalCharsLen, output.Length - totalCharsLen, i == lastIndex, out bytesUsed, out charsUsed, out completed);
                totalCharsLen += charsUsed;
            }

            return new string(output, 0, totalCharsLen);
        }

        public static string GetString(this Encoding encoding, IList<ArraySegment<byte>> data, int offset, int length)
        {
            var output = new char[encoding.GetMaxCharCount(length)];

            var decoder = encoding.GetDecoder();

            var totalCharsLen = 0;
            var totalBytesLen = 0;
            var lastIndex = data.Count - 1;
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;

            var targetOffset = 0;

            for (var i = 0; i < data.Count; i++)
            {
                var segment = data[i];
                var srcOffset = segment.Offset;
                var srcLength = segment.Count;
                var lastSegment = false;

                //Haven't found the offset position
                if (totalBytesLen == 0)
                {
                    var targetEndOffset = targetOffset + segment.Count - 1;

                    if (offset > targetEndOffset)
                    {
                        targetOffset = targetEndOffset + 1;
                        continue;
                    }

                    //the offset locates in this segment
                    var margin = offset - targetOffset;
                    srcOffset = srcOffset + margin;
                    srcLength = srcLength - margin;

                    if (srcLength >= length)
                    {
                        srcLength = length;
                        lastSegment = true;
                    }
                }
                else
                {
                    var restLength = length - totalBytesLen;

                    if (restLength <= srcLength)
                    {
                        srcLength = restLength;
                        lastSegment = true;
                    }
                }

                decoder.Convert(segment.Array, srcOffset, srcLength, output, totalCharsLen, output.Length - totalCharsLen, lastSegment, out bytesUsed, out charsUsed, out completed);
                totalCharsLen += charsUsed;
                totalBytesLen += bytesUsed;
            }

            return new string(output, 0, totalCharsLen);
        }
    }
}

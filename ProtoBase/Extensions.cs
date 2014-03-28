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
    }
}

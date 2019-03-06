using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{

    public static class Extensions
    {
        public static string GetString(this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment)
            {
                return encoding.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    var count = encoding.GetChars(segment.Span, span);
                    span = span.Slice(count);
                }
            });
        }
    }
}
using System;
using System.Buffers;
using System.IO.Pipelines;
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

        public static int Write(this PipeWriter writer, ReadOnlySpan<char> text, Encoding encoding)
        {
            var encoder = encoding.GetEncoder();
            var completed = false;
            var totalBytes = 0;

            while (!completed)
            {
                var span = writer.GetSpan();

                encoder.Convert(text, span, false, out int charsUsed, out int bytesUsed, out completed);
                
                if (charsUsed > 0)
                    text = text.Slice(charsUsed);

                totalBytes += bytesUsed;
                writer.Advance(bytesUsed);
            }

            return totalBytes;
        }
    }
}
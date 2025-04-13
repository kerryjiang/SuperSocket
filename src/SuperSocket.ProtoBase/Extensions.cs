using System;
using System.Buffers;
using System.Text;
using System.Buffers.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Provides utility extension methods for working with sequences and buffers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Reads a string from the sequence reader using UTF-8 encoding.
        /// </summary>
        /// <param name="reader">The sequence reader.</param>
        /// <param name="length">The length of the string to read. If 0, reads the remaining length.</param>
        /// <returns>The decoded string.</returns>
        public static string ReadString(ref this SequenceReader<byte> reader, long length = 0)
        {
            return ReadString(ref reader, Encoding.UTF8, length);
        }

        /// <summary>
        /// Reads a string from the sequence reader using the specified encoding.
        /// </summary>
        /// <param name="reader">The sequence reader.</param>
        /// <param name="encoding">The encoding to use for decoding the string.</param>
        /// <param name="length">The length of the string to read. If 0, reads the remaining length.</param>
        /// <returns>The decoded string.</returns>
        public static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding, long length = 0)
        {
            if (length == 0)
                length = reader.Remaining;

            var seq = reader.Sequence.Slice(reader.Consumed, length);

            try
            {
                return seq.GetString(encoding);
            }
            finally
            {
                reader.Advance(length);
            }
        }

        /// <summary>
        /// Attempts to read a 16-bit unsigned integer in big-endian format from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader.</param>
        /// <param name="value">The read value.</param>
        /// <returns><c>true</c> if the value was successfully read; otherwise, <c>false</c>.</returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ushort value)
        {
            value = 0;

            if (reader.Remaining < 2)
                return false;

            if (!reader.TryRead(out byte h))
                return false;

            if (!reader.TryRead(out byte l))
                return false;

            value = (ushort)(h * 256 + l);
            return true;
        }

        /// <summary>
        /// Attempts to read a 32-bit unsigned integer in big-endian format from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader.</param>
        /// <param name="value">The read value.</param>
        /// <returns><c>true</c> if the value was successfully read; otherwise, <c>false</c>.</returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out uint value)
        {
            value = 0;

            if (reader.Remaining < 4)
                return false;

            var v = 0;
            var unit = (int)Math.Pow(256, 3);

            for (var i = 0; i < 4; i++)
            {
                if (!reader.TryRead(out byte b))
                    return false;

                v += unit * b;
                unit = unit / 256;
            }

            value = (uint)v;
            return true;
        }

        /// <summary>
        /// Attempts to read a 64-bit unsigned integer in big-endian format from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader.</param>
        /// <param name="value">The read value.</param>
        /// <returns><c>true</c> if the value was successfully read; otherwise, <c>false</c>.</returns>
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ulong value)
        {
            value = 0;

            if (reader.Remaining < 8)
                return false;

            var v = 0L;
            var unit = (long)Math.Pow(256, 7);

            for (var i = 0; i < 8; i++)
            {
                if (!reader.TryRead(out byte b))
                    return false;

                v += unit * b;
                unit = unit / 256;
            }

            value = (ulong)v;
            return true;
        }

        /// <summary>
        /// Converts a read-only sequence of bytes to a string using the specified encoding.
        /// </summary>
        /// <param name="buffer">The read-only sequence of bytes.</param>
        /// <param name="encoding">The encoding to use for decoding the string.</param>
        /// <returns>The decoded string.</returns>
        public static string GetString(this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment)
            {
                return encoding.GetString(buffer.First.Span);
            }

            if (encoding.IsSingleByte)
            {
                return string.Create((int)buffer.Length, buffer, (span, sequence) =>
                {
                    foreach (var segment in sequence)
                    {
                        var count = encoding.GetChars(segment.Span, span);
                        span = span.Slice(count);
                    }
                });
            }

            var sb = new StringBuilder();
            var decoder = encoding.GetDecoder();

            foreach (var piece in buffer)
            {
                var charBuff = (new char[piece.Length]).AsSpan();
                var len = decoder.GetChars(piece.Span, charBuff, false);
                sb.Append(new string(len == charBuff.Length ? charBuff : charBuff.Slice(0, len)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified text to the buffer writer using the specified encoding.
        /// </summary>
        /// <param name="writer">The buffer writer.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="encoding">The encoding to use for encoding the text.</param>
        /// <returns>The total number of bytes written to the buffer writer.</returns>
        public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
        {
            var encoder = encoding.GetEncoder();
            var completed = false;
            var totalBytes = 0;

            var minSpanSizeHint = encoding.GetMaxByteCount(1);

            while (!completed)
            {
                var span = writer.GetSpan(minSpanSizeHint);

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
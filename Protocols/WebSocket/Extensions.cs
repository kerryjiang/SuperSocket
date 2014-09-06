using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Extension class
    /// </summary>
    public static partial class Extensions
    {
        private readonly static char[] m_CrCf;

        static Extensions()
        {
            m_CrCf = "\r\n".ToArray();
        }

        /// <summary>
        /// Appends in the format with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg">The arg.</param>
        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends in the format with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="content">The content.</param>
        public static void AppendWithCrCf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void AppendWithCrCf(this StringBuilder builder)
        {
            builder.Append(m_CrCf);
        }

        private static Type[] m_SimpleTypes = new Type[] { 
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            };

        internal static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                m_SimpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        /// <summary>
        /// Decodes data by the mask.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="offset">The offset, where the decoding should start from in the data source.</param>
        /// <param name="length">How long the data should be decoded.</param>
        public static void DecodeMask(IList<ArraySegment<byte>> source, byte[] mask, int offset, int length)
        {
            int maskLen = mask.Length;
            var from = 0;
            var startSegmentIndex = 0;
            ArraySegment<byte> startSegment = default(ArraySegment<byte>);

            var totalOffset = 0;

            for (var i = 0; i < source.Count; i++)
            {
                var segment = source[i];

                var nextTotalOffset = totalOffset + segment.Count;

                if (offset >= totalOffset)
                {
                    totalOffset = nextTotalOffset;
                    continue;
                }

                startSegment = segment;
                startSegmentIndex = i;
                from = offset - totalOffset + segment.Offset;
                break;
            }

            var shouldDecode = Math.Min(length, startSegment.Count - from + offset);
            var index = 0;

            for (var i = from; i < from + shouldDecode; i++)
            {
                startSegment.Array[i] = (byte)(startSegment.Array[i] ^ mask[index++ % maskLen]);
            }

            if (index >= length)
                return;

            for (var i = startSegmentIndex + 1; i < source.Count; i++)
            {
                var segment = source[i];

                shouldDecode = Math.Min(length - index, segment.Count);

                for (var j = segment.Offset; j < segment.Offset + shouldDecode; j++)
                {
                    segment.Array[j] = (byte)(segment.Array[j] ^ mask[index++ % maskLen]);
                }

                if (index >= length)
                    return;
            }
        }
    }
}

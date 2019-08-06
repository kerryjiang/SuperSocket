using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.WebSocket
{
    public static partial class Extensions
    {
        private readonly static char[] m_CrCf = new char[] { '\r', '\n' };

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
    }
}

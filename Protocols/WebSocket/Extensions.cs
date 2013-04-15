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
    }
}

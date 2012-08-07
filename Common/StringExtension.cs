using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    /// <summary>
    /// String extension class
    /// </summary>
    public static partial class StringExtension
    {
        /// <summary>
        /// Convert string to int32.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static int ToInt32(this string source)
        {
            return source.ToInt32(0);
        }

        /// <summary>
        /// Convert string to int32.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int ToInt32(this string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            int value;

            if (!int.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to long.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static long ToLong(this string source)
        {
            return source.ToLong(0);
        }

        /// <summary>
        /// Convert string to long.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static long ToLong(this string source, long defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            long value;

            if (!long.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to short.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static short ToShort(this string source)
        {
            return source.ToShort(0);
        }

        /// <summary>
        /// Convert string to short.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static short ToShort(this string source, short defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            short value;

            if (!short.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to decimal.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source)
        {
            return source.ToDecimal(0);
        }

        /// <summary>
        /// Convert string to decimal.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source, decimal defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            decimal value;

            if (!decimal.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to date time.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string source)
        {
            return source.ToDateTime(DateTime.MinValue);
        }

        /// <summary>
        /// Convert string to date time.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string source, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            DateTime value;

            if (!DateTime.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to boolean.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source)
        {
            return source.ToBoolean(false);
        }

        /// <summary>
        /// Convert string tp boolean.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source, bool defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            bool value;

            if (!bool.TryParse(source, out value))
                value = defaultValue;

            return value;
        }
    }
}

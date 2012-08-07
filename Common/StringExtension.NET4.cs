using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    /// <summary>
    /// String extension
    /// </summary>
    public static partial class StringExtension
    {
        /// <summary>
        /// Tries parse string to enum.
        /// </summary>
        /// <typeparam name="T">the enum type</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(this string value, bool ignoreCase, out T enumValue)
            where T : struct
        {
            return Enum.TryParse<T>(value, ignoreCase, out enumValue);
        }
    }
}

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
        /// Tries to parse string to enum type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(this string value, bool ignoreCase, out T enumValue)
            where T : struct
        {
            try
            {
                enumValue = (T)System.Enum.Parse(typeof(T), value, ignoreCase);
                return true;
            }
            catch
            {
                enumValue = default(T);
                return false;
            }
        }
    }
}

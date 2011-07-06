using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public static partial class StringExtension
    {
        public static bool TryParseEnum<T>(this string value, bool ignoreCase, out T enumValue)
            where T : struct
        {
            return Enum.TryParse<T>(value, ignoreCase, out enumValue);
        }
    }
}

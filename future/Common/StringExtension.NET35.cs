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

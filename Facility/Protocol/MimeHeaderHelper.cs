using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// MimeHeader Helper class
    /// </summary>
    public static class MimeHeaderHelper
    {
        private const string Tab = "\t";
        private const char Colon = ':';
        private const string Space = " ";
        private const string ValueSeparator = ", ";

        /// <summary>
        /// Parses the HTTP header.
        /// </summary>
        /// <param name="headerData">The header data.</param>
        /// <param name="header">The header.</param>
        public static void ParseHttpHeader(string headerData, NameValueCollection header)
        {
            string line;
            string firstLine = string.Empty;
            string prevKey = string.Empty;

            var reader = new StringReader(headerData);

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                if (line.StartsWith(Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    string currentValue = header[prevKey];
                    header[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(Colon);

                if (pos <= 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                var valueOffset = pos + 1;

                if (line.Length <= valueOffset) //No value in this line
                    continue;

                string value = line.Substring(valueOffset);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                string oldValue = header[key];

                if (string.IsNullOrEmpty(oldValue))
                {
                    header.Add(key, value);
                }
                else
                {
                    header[key] = oldValue + ValueSeparator + value;
                }

                prevKey = key;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// MimeHeader Helper class
    /// </summary>
    public static class MimeHeaderHelper
    {
        private const string m_Tab = "\t";
        private const char m_Colon = ':';
        private const string m_Space = " ";
        private const char m_SpaceChar = ' ';
        private const string m_ValueSeparator = ", ";

        /// <summary>
        /// Parses the HTTP header.
        /// </summary>
        /// <param name="headerData">The header data.</param>
        /// <param name="header">The header.</param>
        public static void ParseHttpHeader(string headerData, HttpHeaderInfo header)
        {
            string line;
            string firstLine = string.Empty;
            string prevKey = string.Empty;
            string prevValue = string.Empty;

            var reader = new StringReader(headerData);

            var keyHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                if (line.StartsWith(m_Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    prevValue = prevValue + line.Trim();
                    header[prevKey] = prevValue;
                    continue;
                }

                int pos = line.IndexOf(m_Colon);

                if (pos <= 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                var valueOffset = pos + 1;

                if (line.Length <= valueOffset) //No value in this line
                    continue;

                string value = line.Substring(valueOffset);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(m_Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                if (keyHash.Contains(key))
                {
                    prevValue = header[key] + m_ValueSeparator + value;
                    header[key] = prevValue;
                }
                else
                {
                    header[key] = prevValue = value;
                }

                prevKey = key;
            }

            var metaInfo = firstLine.Split(m_SpaceChar);

            header.Method = metaInfo[0];
            header.Path = metaInfo[1];
            header.Version = metaInfo[2];
        }
    }
}

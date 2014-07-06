using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// the interface for the tools to parse string into key, body and parameters parts
    /// </summary>
    public interface IStringParser
    {
        /// <summary>
        /// parse the source string into key, body and parameters parts
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="key">the parsed key</param>
        /// <param name="body">the parsed body</param>
        /// <param name="parameters">the parsed parameter</param>
        void Parse(string source, out string key, out string body, out string[] parameters);
    }
}

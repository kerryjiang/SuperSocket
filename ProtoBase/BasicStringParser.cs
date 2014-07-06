using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// the string parser who use SPACE to separate string into many parts
    /// </summary>
    public class BasicStringParser : IStringParser
    {
        private const string SPACE = " ";

        private readonly string m_Spliter;
        private readonly string[] m_ParameterSpliters;

        /// <summary>
        /// The default singlegton instance
        /// </summary>
        public static readonly BasicStringParser DefaultInstance = new BasicStringParser();

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicStringParser"/> class.
        /// </summary>
        public BasicStringParser()
            : this(SPACE, SPACE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicStringParser"/> class.
        /// </summary>
        /// <param name="spliter">The spliter between command name and command parameters.</param>
        /// <param name="parameterSpliter">The parameter spliter.</param>
        public BasicStringParser(string spliter, string parameterSpliter)
        {
            m_Spliter = spliter;
            m_ParameterSpliters = new string[] { parameterSpliter };
        }

        /// <summary>
        /// parse the source string into key, body and parameters parts
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="key">the parsed key</param>
        /// <param name="body">the parsed body</param>
        /// <param name="parameters">the parsed parameter</param>
        public void Parse(string source, out string key, out string body, out string[] parameters)
        {
            int pos = source.IndexOf(m_Spliter);

            if (pos > 0)
            {
                key = source.Substring(0, pos);
                body = source.Substring(pos + 1);
                parameters = body.Split(m_ParameterSpliters, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                key = body = source;
                parameters = null;
            }
        }
    }
}

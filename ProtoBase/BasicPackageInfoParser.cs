using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Basic package info parser, which parse package info by separating
    /// </summary>
    public class BasicPackageInfoParser : IStringPackageParser<StringPackageInfo>
    {
        private readonly string m_Spliter;
        private readonly string[] m_ParameterSpliters;

        private const string m_OneSpace = " ";

        /// <summary>
        /// The default singlegton instance
        /// </summary>
        public static readonly BasicPackageInfoParser DefaultInstance = new BasicPackageInfoParser();

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicPackageInfoParser"/> class.
        /// </summary>
        public BasicPackageInfoParser()
            : this(m_OneSpace, m_OneSpace)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicPackageInfoParser"/> class.
        /// </summary>
        /// <param name="spliter">The spliter between command name and command parameters.</param>
        /// <param name="parameterSpliter">The parameter spliter.</param>
        public BasicPackageInfoParser(string spliter, string parameterSpliter)
        {
            m_Spliter = spliter;
            m_ParameterSpliters = new string[] { parameterSpliter };
        }

        #region IPackageInfoParser<StringPackageInfo> Members

        /// <summary>
        /// Parses the request info.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public StringPackageInfo Parse(string source)
        {
            int pos = source.IndexOf(m_Spliter);

            string name = string.Empty;
            string param = string.Empty;

            if (pos > 0)
            {
                name = source.Substring(0, pos);
                param = source.Substring(pos + 1);
            }
            else
            {
                name = source;
            }

            return new StringPackageInfo(name, param,
                param.Split(m_ParameterSpliters, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}

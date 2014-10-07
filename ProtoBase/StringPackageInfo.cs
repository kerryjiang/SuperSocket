using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// String package info class
    /// </summary>
    public class StringPackageInfo : IPackageInfo<string>
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; protected set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; protected set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string[] Parameters { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPackageInfo"/> class.
        /// </summary>
        protected StringPackageInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPackageInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        public StringPackageInfo(string key, string body, string[] parameters)
        {
            Key = key;
            Body = body;
            Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPackageInfo"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceParser">The source parser.</param>
        public StringPackageInfo(string source, IStringParser sourceParser)
        {
            InitializeData(source, sourceParser);
        }

        /// <summary>
        /// Initializes the string package's data.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="sourceParser">The source parser.</param>
        protected void InitializeData(string source, IStringParser sourceParser)
        {
            string key;
            string body;
            string[] parameters;

            sourceParser.Parse(source, out key, out body, out parameters);

            Key = key;
            Body = body;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the first param.
        /// </summary>
        /// <returns></returns>
        public string GetFirstParam()
        {
            if (Parameters.Length > 0)
                return Parameters[0];

            return string.Empty;
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> at the specified index.
        /// </summary>
        public string this[int index]
        {
            get { return Parameters[index]; }
        }
    }
}

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
        public string Key { get; private set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string[] Parameters { get; private set; }

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

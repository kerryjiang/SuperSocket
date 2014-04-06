using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// String type request information
    /// </summary>
    public class StringRequestInfo : StringPackageInfo, IRequestInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringRequestInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        public StringRequestInfo(string key, string body, string[] parameters)
            : base(key, body, parameters)
        {

        }

        /// <summary>
        /// Gets the first param.
        /// </summary>
        /// <returns></returns>
        public string GetFirstParam()
        {
            if(Parameters.Length > 0)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// Status information metadata attribute
    /// </summary>
    public class StatusInfoAttribute : DisplayAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusInfoAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StatusInfoAttribute(string name)
            : base(name)
        {
            OutputInPerfLog = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusInfoAttribute" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        public StatusInfoAttribute(string key, string name)
            : base(name)
        {
            Key = key;
            OutputInPerfLog = true;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [output in perf log].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [output in perf log]; otherwise, <c>false</c>.
        /// </value>
        public bool OutputInPerfLog { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public Type DataType { get; set; }
    }
}

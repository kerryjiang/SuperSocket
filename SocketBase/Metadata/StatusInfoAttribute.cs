using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// StatusInfo Metadata
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StatusInfoAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusInfoAttribute" /> class.
        /// </summary>
        public StatusInfoAttribute()
        {
            OutputInPerfLog = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusInfoAttribute" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public StatusInfoAttribute(string key)
            : this()
        {
            Key = key;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }


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

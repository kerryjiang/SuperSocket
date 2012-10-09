using System;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Display attribute
    /// </summary>
    public class DisplayAttribute : Attribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="DisplayAttribute" /> class.
        /// </summary>
        public DisplayAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public DisplayAttribute(string name)
        {
            Name = name;
            OutputInPerfLog = true;
        }
    }
}

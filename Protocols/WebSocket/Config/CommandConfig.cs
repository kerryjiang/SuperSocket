using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.WebSocket.Config
{
    /// <summary>
    /// Command configuration
    /// </summary>
    public class CommandConfig : ConfigurationElement
    {
        /// <summary>
        /// Gets the options.
        /// </summary>
        public NameValueCollection Options { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (Options == null)
            {
                Options = new NameValueCollection();
            }

            Options.Add(name, value);
            return true;
        }
    }
}

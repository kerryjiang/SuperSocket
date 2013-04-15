using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Text encoding binary data converter
    /// </summary>
    public class TextEncodingBinaryDataConverter : IBinaryDataConverter
    {
        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEncodingBinaryDataConverter" /> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public TextEncodingBinaryDataConverter(Encoding encoding)
        {
            Encoding = encoding;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(byte[] data, int offset, int length)
        {
            return Encoding.GetString(data, offset, length);
        }
    }
}

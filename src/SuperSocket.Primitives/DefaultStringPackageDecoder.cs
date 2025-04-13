using System;
using System.Buffers;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    /// <summary>
    /// Decodes byte sequences into <see cref="StringPackageInfo"/> objects using a specified encoding.
    /// </summary>
    public class DefaultStringPackageDecoder : IPackageDecoder<StringPackageInfo>
    {
        /// <summary>
        /// Gets the encoding used for decoding.
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStringPackageDecoder"/> class with UTF-8 encoding.
        /// </summary>
        public DefaultStringPackageDecoder()
            : this(new UTF8Encoding(false))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStringPackageDecoder"/> class with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for decoding.</param>
        public DefaultStringPackageDecoder(Encoding encoding)
        {
            Encoding = encoding;
        }

        /// <summary>
        /// Decodes the specified byte sequence into a <see cref="StringPackageInfo"/> object.
        /// </summary>
        /// <param name="buffer">The byte sequence to decode.</param>
        /// <param name="context">The context for decoding (optional).</param>
        /// <returns>The decoded <see cref="StringPackageInfo"/> object.</returns>
        public StringPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
        {
            var text = buffer.GetString(Encoding);
            var parts = text.Split(' ', 2);

            var key = parts[0];

            if (parts.Length <= 1)
            {
                return new StringPackageInfo
                {
                    Key = key
                };
            }

            return new StringPackageInfo
            {
                Key = key,
                Body = parts[1],
                Parameters = parts[1].Split(' ')
            };
        }
    }
}
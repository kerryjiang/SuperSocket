using System;
using System.Buffers;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public class DefaultStringPackageDecoder : IPackageDecoder<StringPackageInfo>
    {
        public Encoding Encoding { get; private set; }

        public DefaultStringPackageDecoder()
            : this(new UTF8Encoding(false))
        {

        }

        public DefaultStringPackageDecoder(Encoding encoding)
        {
            Encoding = encoding;
        }

        public StringPackageInfo Decode(ReadOnlySequence<byte> buffer, object context)
        {
            var text = buffer.GetString(Encoding);
            var parts = text.Split(' ');

            return new StringPackageInfo
            {
                Key = parts[0],
                Body = text,
                Parameters = parts.Skip(1).ToArray()
            };
        }
    }
}
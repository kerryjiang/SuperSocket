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

        public StringPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
        {
            var text = buffer.GetString(Encoding);
            var parts = text.Split(' ', 2);

            return new StringPackageInfo
            {
                Key = parts[0],
                Body = parts[1],
                Parameters = parts[1].Split(' ')
            };
        }
    }
}
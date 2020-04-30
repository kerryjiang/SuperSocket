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
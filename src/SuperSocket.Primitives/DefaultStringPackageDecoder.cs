using System;
using System.Buffers;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public class DefaultStringPackageDecoder : IPackageDecoder<StringPackageInfo>
    {
        public StringPackageInfo Decode(ReadOnlySequence<byte> buffer)
        {
            var text = buffer.GetString(Encoding.UTF8);
            var parts = text.Split(' ');

            return new StringPackageInfo
            {
                Key = parts[0],
                Parameters = parts.Skip(1).ToArray()
            };
        }
    }
}
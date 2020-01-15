using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public class DefaultStringEncoder : IPackageEncoder<string>
    {
        private Encoding _encoding;

        public DefaultStringEncoder()
            : this(new UTF8Encoding(false))
        {

        }

        public DefaultStringEncoder(Encoding encoding)
        {
            _encoding = encoding;
        }

        public int Encode(IBufferWriter<byte> writer, string pack)
        {
            return writer.Write(pack, _encoding);
        }
    }
}
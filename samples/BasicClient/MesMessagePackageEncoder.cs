using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace BasicClient
{
    public class MesMessagePackageEncoder : IPackageEncoder<MesMessage>
    {
        public int Encode(IBufferWriter<byte> writer, MesMessage pack)
        {
            var bytes = Encoding.UTF8.GetBytes(pack.ToString());
            var span = new ReadOnlySpan<byte>(bytes);
            writer.Write(span);
            return bytes.Length;
        }
    }
}

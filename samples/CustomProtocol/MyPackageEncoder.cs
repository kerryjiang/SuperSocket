using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace CustomProtocol
{
    public class MyPackageEncoder : IPackageEncoder<MyPackage>
    {
        public int Encode(IBufferWriter<byte> writer, MyPackage pack)
        {
            writer.Write(new ReadOnlySpan<byte>(new byte[] { (byte)pack.Code }));
            writer.Write(UInt16ToBytes((UInt16)(pack.Body.Length)));
            writer.Write(UInt16ToBytes((UInt16)(pack.Sequence)));
            writer.Write(Encoding.ASCII.GetBytes(pack.Body));
            return 5 + pack.Body.Length;
        }
        public static byte[] UInt16ToBytes(UInt16 data)
        {
            var bytes = new byte[2];
            bytes[0] = (byte)((data & 0xFFFF) >> (1 * 0x08));
            bytes[1] = (byte)((data & 0xFFFF) << (1 * 0x08) >> (1 * 0x08)) ;
            return bytes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyReceiveFilter : FixedHeaderReceiveFilter<BufferedPackageInfo>
    {
        public MyReceiveFilter()
            : base(6)
        {

        }

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            bufferStream.Skip(4);
            return bufferStream.ReadUInt16();
        }

        public override BufferedPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var key = bufferStream.ReadString(4, Encoding.UTF8);
            bufferStream.Skip(2);
            return new BufferedPackageInfo(key, bufferStream.Take((int)bufferStream.Length - 6));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyReceiveFilter : FixedHeaderReceiveFilter<BinaryRequestInfo>
    {
        public MyReceiveFilter()
            : base(6)
        {

        }

        protected override int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length)
        {
            using (var reader = this.GetBufferReader(packageData))
            {
                reader.Skip(4);
                return reader.ReadUInt16();
            }
        }

        public override BinaryRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            using (var reader = this.GetBufferReader(packageData))
            {
                var key = reader.ReadString(4, Encoding.UTF8);
                reader.Skip(2);
                return new BinaryRequestInfo(key, reader.Take((int)reader.Length - 6));
            }
        }
    }
}

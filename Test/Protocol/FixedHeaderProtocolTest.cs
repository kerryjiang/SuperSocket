using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    [TestFixture]
    public class FixedHeaderProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : FixedHeaderReceiveFilter<StringPackageInfo>
        {
            public TestReceiveFilter()
                : base(8)
            {

            }

            protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
            {
                var strLen = bufferStream.Skip(4).ReadString(4, Encoding.ASCII);
                return int.Parse(strLen.TrimStart('0'));
            }

            public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
            {
                var total = (int)bufferStream.Length;
                var key = bufferStream.ReadString(4, Encoding.ASCII);
                bufferStream.Skip(4); // skip length part
                var body = bufferStream.ReadString(total - HeaderSize, Encoding.ASCII);
                return new StringPackageInfo(key, body, new string[] { body });
            }
        }

        protected override IReceiveFilterFactory<StringPackageInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringPackageInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("ECHO{0}{1}", sourceLine.Length.ToString().PadLeft(4), sourceLine);
        }
    }
}

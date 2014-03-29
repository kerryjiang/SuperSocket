using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test.Protocol
{
    [TestFixture]
    public class FixedHeaderProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : FixedHeaderReceiveFilter<StringRequestInfo>
        {
            public TestReceiveFilter()
                : base(8)
            {

            }

            protected override int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length)
            {
                var strLen = Encoding.ASCII.GetString(packageData, 4, 4);
                return int.Parse(strLen.TrimStart('0'));
            }

            public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var total = packageData.Sum(x => x.Count);
                var body = Encoding.ASCII.GetString(packageData, HeaderSize, total - HeaderSize);
                return new StringRequestInfo(Encoding.ASCII.GetString(packageData, 0, 4), body, new string[] { body });
            }
        }

        protected override IReceiveFilterFactory<StringRequestInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringRequestInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("ECHO{0}{1}", sourceLine.Length.ToString().PadLeft(4), sourceLine);
        }
    }
}

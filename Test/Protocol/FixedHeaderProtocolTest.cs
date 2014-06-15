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

            protected override int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length)
            {
                var strLen = Encoding.ASCII.GetString(packageData, 4, 4);
                return int.Parse(strLen.TrimStart('0'));
            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var total = packageData.Sum(x => x.Count);
                var body = Encoding.ASCII.GetString(packageData, HeaderSize, total - HeaderSize);
                return new StringPackageInfo(Encoding.ASCII.GetString(packageData, 0, 4), body, new string[] { body });
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

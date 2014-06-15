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
    public class CountSpliterProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : CountSpliterReceiveFilter<StringPackageInfo>
        {
            public TestReceiveFilter()
                : base(new byte[] { (byte)'#' }, 3)
            {

            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var line = Encoding.ASCII.GetString(packageData);
                var arr = line.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                return new StringPackageInfo(arr[0], arr[1], new string[] { arr[1] });
            }
        }

        protected override IReceiveFilterFactory<StringPackageInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringPackageInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("#ECHO#{0}#", sourceLine);
        }
    }
}

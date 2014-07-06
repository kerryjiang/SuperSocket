using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    [TestFixture]
    public class BeginEndMarkProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : BeginEndMarkReceiveFilter<StringPackageInfo>
        {
            private readonly static byte[] BeginMark = new byte[] { 0x5b, 0x5b };
            private readonly static byte[] EndMark = new byte[] { 0x5d, 0x5d };

            private BasicStringParser m_Parser = new BasicStringParser();

            public TestReceiveFilter()
                : base(BeginMark, EndMark)
            {

            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                using (var reader = this.GetBufferReader(packageData))
                {
                    var length = reader.Length;

                    if (length < 20)
                    {
                        Console.WriteLine("Ignore request");
                        return null;
                    }

                    return new StringPackageInfo(reader.Skip(2).ReadString((int)length - 4, Encoding.ASCII), m_Parser);
                }
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("[[ECHO {0}]]", sourceLine);
        }

        [Test]
        public void TestProgramReturnRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Encoding.ASCII, true))
                using (var writer = new ConsoleWriter(socketStream, Encoding.ASCII, 1024 * 8))
                {
                    reader.ReadLine();

                    var line = "1234]]" + Guid.NewGuid().ToString();
                    writer.Write(CreateRequest(line));
                    writer.Flush();

                    var receivedLine = reader.ReadLine();

                    Assert.AreEqual(line, receivedLine);

                    line = "1234]]" + Guid.NewGuid().ToString();
                    var request = CreateRequest(line);

                    for (var i = 0; i < request.Length; i++)
                    {
                        writer.Write(request[i]);
                        writer.Flush();
                        Thread.Sleep(50);
                    }

                    receivedLine = reader.ReadLine();
                    Assert.AreEqual(line, receivedLine);
                }
            }
        }

        protected override IReceiveFilterFactory<StringPackageInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringPackageInfo>(); }
        }
    }
}

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using SuperSocket;
using SuperSocket.ProtoBase;
using Tests;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Trait("Category", "Protocol.XLCustom")]
    public class XLCustomProtocalTest : ProtocolTestBase
    {
        public XLCustomProtocalTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {

        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override IServer CreateServer()
        {
            return CreateSocketServerBuilder<TextPackageInfo, XLCustomFilter>().ConfigureSuperSocket((options) =>
            {
                options.MaxPackageLength = 8 * 1024 * 1024;
                options.ReceiveBufferSize = 8 * 1024 * 1024;
            })
                .ConfigurePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer() as IServer;
        }



        public class XLCustomFilter : TerminatorPipelineFilter<TextPackageInfo>
        {
            public XLCustomFilter()
                : base(new byte[] { (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F', (byte)'F' })
            {
            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.Slice(4).GetString(Utf8Encoding) };
            }
        }

        [Fact]
        [Trait("Category", "Protocol. XLCustomProtocal.TestBatchRequest")]
        public override void TestBatchRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 1024 * 8))
                {
                    int size = 100;

                    var lines = new string[size];

                    for (var i = 0; i < size; i++)
                    {
                        var line = Guid.NewGuid().ToString();
                        for (int j = 0; j < 1000; j++)
                        {
                            line += Guid.NewGuid().ToString();
                        }
                        lines[i] = line;
                        var request = CreateRequest(line + "FFFFFFFFFFFF");
                        writer.Write(request);
                    }

                    writer.Flush();

                    for (var i = 0; i < size; i++)
                    {
                        var receivedLine = reader.ReadLine();
                        Assert.Equal(lines[i], receivedLine);
                    }
                }
            }
        }
    }
}

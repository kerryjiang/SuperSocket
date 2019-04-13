using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Protocol.Terminator")]
    public class TerminatorProtocolTest : ProtocolTestBase
    {
        public TerminatorProtocolTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {

        }

        protected override string CreateRequest(string sourceLine)
        {
            return $"{sourceLine}##";
        }

        protected override IServer CreateServer()
        {
            var server = CreateSocketServerBuilder<TextPackageInfo>((x) => new TerminatorTextPipelineFilter(new[] { (byte)'#', (byte)'#' }))
                .ConfigurePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
                }).Build() as IServer;

            return server;
        }
    }
}

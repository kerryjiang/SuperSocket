using System;
using System.Text;
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

        protected override SuperSocketServer CreateServer()
        {
            return CreateSocketServer(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            }, pipeLineFilterFactory: (x) => new TerminatorTextPipelineFilter(new[] { (byte)'#', (byte)'#' }));
        }
    }
}

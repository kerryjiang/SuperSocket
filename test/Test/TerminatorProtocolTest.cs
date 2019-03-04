using System;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
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
            return string.Format("{0}##", sourceLine);
        }

        protected override SuperSocketServer CreateSevrer()
        {
            return CreateSocketServer<TextPackageInfo>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            }, pipeLineFilterFactory: (x) => new TerminatorTextPipelineFilter(new byte[] { (byte)'#', (byte)'#' }));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class TerminatorProtocolTest : ProtocolTestBase
    {
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

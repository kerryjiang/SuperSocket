using System;
using System.Buffers;
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
    [Collection("Protocol.FixedSize")]
    public class FixedSizeProtocolTest : ProtocolTestBase
    {
        class MyFixedSizePiplelineFilter : FixedSizePipelineFilter<TextPackageInfo>
        {
            public MyFixedSizePiplelineFilter()
                : base(36)
            {

            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine;
        }

        protected override SuperSocketServer CreateSevrer()
        {
            return CreateSocketServer<TextPackageInfo, MyFixedSizePiplelineFilter>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            });
        }
    }
}

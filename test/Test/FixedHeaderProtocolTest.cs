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
    [Collection("Protocol.FixedHeader")]
    public class FixedHeaderProtocolTest : ProtocolTestBase
    {
        class MyFixedHeaderPiplelineFilter : FixedHeaderPipelineFilter<TextPackageInfo>
        {
            public MyFixedHeaderPiplelineFilter()
                : base(4)
            {

            }

            protected override int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer)
            {
                var strLen = buffer.GetString(Encoding.ASCII);
                return int.Parse(strLen.TrimStart('0'));
            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.Slice(4).GetString(Encoding.UTF8) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override SuperSocketServer CreateSevrer()
        {
            return CreateSocketServer<TextPackageInfo, MyFixedHeaderPiplelineFilter>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            });
        }
    }
}

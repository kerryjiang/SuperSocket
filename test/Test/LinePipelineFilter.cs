using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class LinePipelineFilter : IPipelineFilter<LinePackageInfo>
    {
        public IPipelineFilter<LinePackageInfo> NextFilter => this;

        public LinePackageInfo Filter(ref ReadOnlyBuffer<byte> buffer)
        {
            return new LinePackageInfo
            {
                Line = Encodings.Utf8.ToString(buffer.ToSpan())
            };
        }
    }
}

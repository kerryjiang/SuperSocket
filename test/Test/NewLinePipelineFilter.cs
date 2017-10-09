using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO.Pipelines.Text.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class NewLinePipelineFilter : TerminatorPipelineFilter<LinePackageInfo>
    {

        public NewLinePipelineFilter()
            : base(new byte[] { (byte)'\r', (byte)'\n' })
        {

        }

        public override LinePackageInfo ResolvePackage(ReadableBuffer buffer)
        {
            return new LinePackageInfo { Line = buffer.GetUtf8String() };
        }
    }
}

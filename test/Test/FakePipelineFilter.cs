using System;
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
    public class FakePipelineFilter : IPipelineFilter<FakePackageInfo>
    {
        public IPipelineFilter<FakePackageInfo> NextFilter => throw new NotImplementedException();

        public FakePackageInfo Filter(ref ReadableBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}

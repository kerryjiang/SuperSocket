using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using SuperSocket.Server;
using System.Threading;
using SuperSocket.Tests.Command;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace SuperSocket.Tests
{
    public static class Extensions
    {
        internal static IPEndPoint GetServerEndPoint(this IHostConfigurator hostConfigurator)
        {
            return new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port);
        }
    }
}

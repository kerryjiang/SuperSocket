using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Channel;

namespace SuperSocket.GZip
{
    public static class HostBuilderExtensions
    {

        // move to extensions
        public static ISuperSocketHostBuilder UseGZip(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder.UseChannelCreatorFactory<GZipTcpChannelCreatorFactory>();
        }

    }
}

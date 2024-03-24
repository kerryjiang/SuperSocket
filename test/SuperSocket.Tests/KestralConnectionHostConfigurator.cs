using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Kestrel;

namespace SuperSocket.Tests
{
    public class KestralConnectionHostConfigurator : RegularHostConfigurator
    {
        public KestralConnectionHostConfigurator()
            : base()
        {
        }

        public override void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            base.Configure(hostBuilder);
            hostBuilder.UseKestrelPipeConnection();
        }
    }
}
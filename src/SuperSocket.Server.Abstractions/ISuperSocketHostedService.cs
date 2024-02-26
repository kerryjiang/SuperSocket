using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions
{
    public interface ISuperSocketHostedService : IHostedService, IServer, IConnectionRegister, ILoggerAccessor, ISessionEventHost
    {
    }
}
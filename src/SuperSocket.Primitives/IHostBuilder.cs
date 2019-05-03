using System;
using Microsoft.Extensions.Hosting;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IHostBuilder<TReceivePackage> : IHostBuilder
        where TReceivePackage : class
    {
        
    }
}
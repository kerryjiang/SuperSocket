using Microsoft.Extensions.Hosting;

namespace SuperSocket
{
    public interface ISuperSocketHostBuilder : IHostBuilder
    {

    }

    public interface ISuperSocketHostBuilder<TPackage> : ISuperSocketHostBuilder
    {
        
    }
}
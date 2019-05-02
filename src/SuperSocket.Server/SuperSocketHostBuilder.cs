using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket
{
/*     public interface IHostBuilder<TReceivePackage, TPipelineFilter> : IHostBuilder
        where TReceivePackage : class
        where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
    {
        
    }

    public class SuperSocketHostBuilder<TReceivePackage, TPipelineFilter> : HostBuilder, IHostBuilder<TReceivePackage, TPipelineFilter>
        where TReceivePackage : class
        where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
    {
        
    } */

    public interface IHostBuilder<TReceivePackage> : IHostBuilder
        where TReceivePackage : class
    {
        
    }

    

    class SuperSocketHostBuilder<TReceivePackage> : HostBuilder, IHostBuilder<TReceivePackage>
        where TReceivePackage : class
    {
        
    }

    public static class SuperSocketHostBuilder
    {
        public static IHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            var hostBuilder = new SuperSocketHostBuilder<TReceivePackage>();
            return hostBuilder.UseSuperSocket<TReceivePackage, TPipelineFilter>() as IHostBuilder<TReceivePackage>;
        }

        public static IHostBuilder<TReceivePackage> Create<TReceivePackage>(Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            var hostBuilder = new SuperSocketHostBuilder<TReceivePackage>();
            return hostBuilder.UseSuperSocket<TReceivePackage>(filterFactory) as IHostBuilder<TReceivePackage>;
        }
    }
}

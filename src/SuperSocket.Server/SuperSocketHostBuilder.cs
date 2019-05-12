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
    public class SuperSocketHostBuilder<TReceivePackage> : HostBuilder, IHostBuilder<TReceivePackage>
        where TReceivePackage : class
    {
        public IHostBuilder<TReceivePackage> ConfigureDefaults()
        {
            return this.ConfigureServices((hostCtx, services) =>
                {
                    // if the package type is StringPackageInfo
                    if (typeof(TReceivePackage) == typeof(StringPackageInfo))
                    {
                        services.AddSingleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>();
                    }

                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                }) as IHostBuilder<TReceivePackage>;
        }
    }

    public static class SuperSocketHostBuilder
    {
        public static IHostBuilder<TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UseSuperSocket<TReceivePackage, TPipelineFilter>() as IHostBuilder<TReceivePackage>;
        }

        public static IHostBuilder<TReceivePackage> Create<TReceivePackage>(Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            return new SuperSocketHostBuilder<TReceivePackage>()
                .ConfigureDefaults()
                .UseSuperSocket<TReceivePackage>(filterFactory) as IHostBuilder<TReceivePackage>;
        }
    }
}

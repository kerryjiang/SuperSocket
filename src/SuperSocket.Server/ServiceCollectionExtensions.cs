using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    /// <summary>
    /// The extensions of <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// The base method for <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TReceivePackage"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSuperSocketBase<TReceivePackage>(this IServiceCollection services, IConfiguration configuration)
            where TReceivePackage : class
        {
            // if the package type is StringPackageInfo
            if (typeof(TReceivePackage) == typeof(StringPackageInfo))
            {
                services.TryAdd(new ServiceDescriptor(typeof(IPackageDecoder<StringPackageInfo>), typeof(DefaultStringPackageDecoder), ServiceLifetime.Singleton));
            }

            services.AddOptions();
            services.Configure<ServerOptions>(configuration.GetSection("serverOptions"));
            services.TryAdd(new ServiceDescriptor(typeof(IPackageEncoder<string>), typeof(DefaultStringEncoderForDI), ServiceLifetime.Singleton));

            return services;
        }

        public static IServiceCollection AddSuperSocket<TReceivePackage, TPipelineFilter>(this IServiceCollection services)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return services.AddSuperSocketWithFilterFactory<TReceivePackage, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
        }

        public static IServiceCollection AddSuperSocket<TReceivePackage, TPipelineFilter, TSuperSocketService>(this IServiceCollection services)
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            return services.AddSuperSocketWithFilterFactory<TReceivePackage, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>, TSuperSocketService>();
        }

        public static IServiceCollection AddSuperSocket<TReceivePackage>(this IServiceCollection services, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
        {
            return services.AddSuperSocket<TReceivePackage, SuperSocketService<TReceivePackage>>(filterFactory);
        }

        public static IServiceCollection AddSuperSocket<TReceivePackage, TSuperSocketService>(this IServiceCollection services, Func<object, IPipelineFilter<TReceivePackage>> filterFactory)
            where TReceivePackage : class
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            services.TryAdd(ServiceDescriptor.Singleton<IChannelCreatorFactory, TcpChannelCreatorFactory>());
            services.AddSingleton<Func<object, IPipelineFilter<TReceivePackage>>>(filterFactory);
            services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DelegatePipelineFilterFactory<TReceivePackage>>();
            services.AddHostedService<TSuperSocketService>();

            return services;
        }


        public static IServiceCollection AddSuperSocketWithFilterFactory<TReceivePackage, TPipelineFilterFactory>(this IServiceCollection services)
            where TReceivePackage : class
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return services.AddSuperSocketWithFilterFactory<TReceivePackage, TPipelineFilterFactory, SuperSocketService<TReceivePackage>>();
        }

        public static IServiceCollection AddSuperSocketWithFilterFactory<TReceivePackage, TPipelineFilterFactory, TSuperSocketService>(this IServiceCollection services)
            where TReceivePackage : class
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
            where TSuperSocketService : SuperSocketService<TReceivePackage>
        {
            services.TryAdd(ServiceDescriptor.Singleton<IChannelCreatorFactory, TcpChannelCreatorFactory>());
            services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
            services.AddHostedService<TSuperSocketService>();

            return services;
        }

        public static IServiceCollection AddSession<TSession>(this IServiceCollection services)
            where TSession : AppSession, new()
        {
            return services.AddSessionFactory<GenericSessionFactory<TSession>>();
        }

        public static IServiceCollection AddSessionFactory<TSessionFactory>(this IServiceCollection services)
            where TSessionFactory : class, ISessionFactory
        {
            services.AddSingleton<ISessionFactory, TSessionFactory>();

            return services;
        }

        public static IServiceCollection AddClearIdleSession(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, ClearIdleSessionMiddleware>());
            return services;
        }

        public static IServiceCollection ConfigurePackageHandler<TReceivePackage>(this IServiceCollection services, Func<IAppSession, TReceivePackage, Task> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            return ConfigurePackageHandlerCore<TReceivePackage>(services, packageHandler, errorHandler: errorHandler);
        }

        private static IServiceCollection ConfigurePackageHandlerCore<TReceivePackage>(IServiceCollection services, Func<IAppSession, TReceivePackage, Task> packageHandler, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
            where TReceivePackage : class
        {
            if (packageHandler == null)
            {
                return services;
            }

            services.AddSingleton<IPackageHandler<TReceivePackage>>(new DelegatePackageHandler<TReceivePackage>(packageHandler));

            if (errorHandler != null)
                services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);

            return services;
        }

        private static IServiceCollection ConfigureErrorHandler<TReceivePackage>(IServiceCollection services, Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
            where TReceivePackage : class
        {
            services.AddSingleton<Func<IAppSession, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);

            return services;
        }

        public static IServiceCollection ConfigurePackageDecoder<TReceivePackage>(this IServiceCollection services, IPackageDecoder<TReceivePackage> packageDecoder)
            where TReceivePackage : class
        {
            return ConfigurePackageDecoderCore<TReceivePackage>(services, packageDecoder);
        }

        private static IServiceCollection ConfigurePackageDecoderCore<TReceivePackage>(IServiceCollection services, IPackageDecoder<TReceivePackage> packageDecoder)
            where TReceivePackage : class
        {

            services.AddSingleton<IPackageDecoder<TReceivePackage>>(packageDecoder);

            return services;
        }

        public static IServiceCollection ConfigureSessionHandler(this IServiceCollection services, Func<IAppSession, ValueTask> onConnected = null, Func<IAppSession, ValueTask> onClosed = null)
        {
            services.AddSingleton<SessionHandlers>(new SessionHandlers
            {
                Connected = onConnected,
                Closed = onClosed
            });

            return services;
        }

        public static IServiceCollection ConfigureSuperSocket(this IServiceCollection services, Action<ServerOptions> configurator)
        {
            services.Configure<ServerOptions>(configurator);

            return services;
        }

        public static IServiceCollection ConfigureSocketOptionsSetter(IServiceCollection services, Func<Socket> socketOptionsSetter)
        {
            services.AddSingleton<Func<Socket>>(socketOptionsSetter);

            return services;
        }
    }
}

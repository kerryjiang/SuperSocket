using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper OutputHelper;

        protected TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {

        }

        protected SuperSocketServer CreateSocketServer<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            return CreateSocketServer<TPackageInfo>(configDict, packageHandler,  new DefaultPipelineFilterFactory<TPackageInfo, TPipelineFilter>());
        }

        protected SuperSocketServer CreateSocketServer<TPackageInfo>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null, Func<object, IPipelineFilter<TPackageInfo>> pipeLineFilterFactory = null)
            where TPackageInfo : class
        {
            return CreateSocketServer<TPackageInfo>(configDict, packageHandler,  new DelegatePipelineFilterFactory<TPackageInfo>(pipeLineFilterFactory));
        }

        protected SuperSocketServer CreateSocketServer<TPackageInfo>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null, IPipelineFilterFactory<TPackageInfo> filterFactory = null)
            where TPackageInfo : class
        {
            if (configDict == null)
            {
                configDict = new Dictionary<string, string>
                {
                    { "serverOptions:name", "TestServer" },
                    { "serverOptions:listeners:0:ip", "Any" },
                    { "serverOptions:listeners:0:port", "4040" }
                };
            }

            var server = new SuperSocketServer();

            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDict);
            var config = builder.Build();
            
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            RegisterServices(services);

            var serverOptions = new ServerOptions();
            config.GetSection("serverOptions").Bind(serverOptions);

            Assert.True(server.Configure<TPackageInfo>(serverOptions, services, packageHandler: packageHandler, pipelineFilterFactory: filterFactory));

            return server;
        }
    }
}

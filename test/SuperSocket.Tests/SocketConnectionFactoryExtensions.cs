using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Tests
{
    public static class SocketConnectionFactoryExtensions
    {
        private const string SocketConnectionFactoryTypeName = "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";

        /// <summary>
        /// 查找SocketConnectionFactory的类型
        /// </summary>
        /// <returns></returns>
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public static Type FindSocketConnectionFactory()
        {
            var assembly = typeof(SocketTransportOptions).Assembly;
            var connectionFactoryType = assembly.GetType(SocketConnectionFactoryTypeName);
            return connectionFactoryType ?? throw new NotSupportedException($"找不到类型{SocketConnectionFactoryTypeName}");
        }
    
        /// <summary>
        /// 注册SocketConnectionFactory为IConnectionFactory
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        [DynamicDependency(DynamicallyAccessedMemberTypes.All,
            typeName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory",
            assemblyName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets")]
        public static IServiceCollection AddSocketConnectionFactory(this IServiceCollection services)
        {
            var factoryType = FindSocketConnectionFactory();

            return services.AddTransient(typeof(IConnectionFactory), factoryType);
        }
    }
}
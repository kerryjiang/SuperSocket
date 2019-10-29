using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class TcpChannelCreatorFactory : IChannelCreatorFactory
    {
        public IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
            where TPackageInfo : class
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

            if (options.Security == SslProtocols.None)
                return new TcpChannelCreator(options, (s) => Task.FromResult((new TcpPipeChannel<TPackageInfo>(s, filterFactory.Create(s), channelOptions)) as IChannel), loggerFactory.CreateLogger(nameof(TcpChannelCreator)));
            else
            {
                var channelFactory = new Func<Socket, Task<IChannel>>(async (s) =>
                {
                    var authOptions = new SslServerAuthenticationOptions();
                    authOptions.EnabledSslProtocols = options.Security;
                    var stream = new SslStream(new NetworkStream(s));
                    await stream.AuthenticateAsServerAsync(authOptions, CancellationToken.None);
                    return new StreamPipeChannel<TPackageInfo>(stream, filterFactory.Create(s), channelOptions);
                });

                return new TcpChannelCreator(options, channelFactory, loggerFactory.CreateLogger(nameof(TcpChannelCreator)));
            }
        }
    }
}
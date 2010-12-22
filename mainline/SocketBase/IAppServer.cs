using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.SocketBase.Command;
using System.ServiceModel.Description;
using SuperSocket.Common;
using System.Security.Authentication;

namespace SuperSocket.SocketBase
{
    public interface ILoggerProvider
    {
        ILogger Logger { get; }
    }

    public interface IAppServer : ILoggerProvider
    {
        string Name { get; }

        ServiceCredentials ServerCredentials { get; set; }

        bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, object protocol, string assembly);

        bool Start();
        
        void Stop();
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        IServerConfig Config { get; }
        X509Certificate Certificate { get; }
        SslProtocols BasicSecurity { get; }
        TAppSession CreateAppSession(ISocketSession socketSession);
        TAppSession GetAppSessionByIndentityKey(string identityKey);
        int SessionCount { get; }
    }

    public interface IAppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession>, ICommandSource<ICommand<TAppSession, TCommandInfo>>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TCommandInfo>
    {
        void ExecuteCommand(TAppSession session, TCommandInfo commandInfo);
    }
}

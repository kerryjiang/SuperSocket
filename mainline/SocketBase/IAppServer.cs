using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.SocketBase.Command;
using System.ServiceModel.Description;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    public interface IAppServer : ILogApp
    {
        ServiceCredentials ServerCredentials { get; set; }

        bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory, object protocol, string consoleBaseAddress, string assembly);

        bool Start();
        
        void Stop();
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        IServerConfig Config { get; }
        X509Certificate Certificate { get; }
        TAppSession CreateAppSession(ISocketSession socketSession);
        TAppSession GetAppSessionByIndentityKey(string identityKey);
        int SessionCount { get; }
    }

    public interface IAppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession>, ICommandSource<ICommand<TAppSession, TCommandInfo>>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TCommandInfo>
    {
        
    }
}

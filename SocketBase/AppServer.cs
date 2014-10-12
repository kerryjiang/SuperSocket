using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// AppServer class
    /// </summary>
    public class AppServer : AppServer<AppSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringPackageInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {

        }
    }

    /// <summary>
    /// AppServer class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public class AppServer<TAppSession> : AppServer<TAppSession, StringPackageInfo>
        where TAppSession : AppSession<TAppSession, StringPackageInfo>, IAppSession, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringPackageInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {

        }

        internal override IReceiveFilterFactory<StringPackageInfo> CreateDefaultReceiveFilterFactory()
        {
            var config = Config;
            
            if(config.Protocol == ProtocolMode.CommandLine)
                return new CommandLineReceiveFilterFactory(TextEncoding);
            else if (config.Protocol == ProtocolMode.WebSocket)
            {
                var websocketReceiveFilterFactoryType =
                    Type.GetType("SuperSocket.WebSocket.WebSocketReceiveFilterFactory, SuperSocket.WebSocket");

                return (IReceiveFilterFactory<StringPackageInfo>)Activator.CreateInstance(websocketReceiveFilterFactoryType);
            }

            return null;
        }
    }
}

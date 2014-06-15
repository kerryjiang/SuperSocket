using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Request handler
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    public delegate void RequestHandler<TAppSession, TPackageInfo>(TAppSession session, TPackageInfo requestInfo)
        where TAppSession : IAppSession, IAppSession<TAppSession, TPackageInfo>, new()
        where TPackageInfo : IPackageInfo;
}

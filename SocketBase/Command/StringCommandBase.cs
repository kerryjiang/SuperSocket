using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// A command type for whose request info type is StringPackageInfo
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public abstract class StringCommandBase<TAppSession> : CommandBase<TAppSession, StringPackageInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringPackageInfo>, new()
    {

    }

    /// <summary>
    /// A command type for whose request info type is StringPackageInfo
    /// </summary>
    public abstract class StringCommandBase : StringCommandBase<AppSession>
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Command Executing Context
    /// </summary>
    public class CommandExecutingContext
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        public IAppSession Session { get; private set; }

        /// <summary>
        /// Gets the request info.
        /// </summary>
        public IPackageInfo Request { get; private set; }

        /// <summary>
        /// Gets the current command.
        /// </summary>
        public ICommand CurrentCommand { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether [exception handled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exception handled]; otherwise, <c>false</c>.
        /// </value>
        public bool ExceptionHandled { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this command executing is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutingContext" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        /// <param name="command">The command.</param>
        public void Initialize(IAppSession session, IPackageInfo requestInfo, ICommand command)
        {
            Session = session;
            Request = requestInfo;
            CurrentCommand = command;
        }
    }

    interface IThreadExecutingContext
    {
        void Increment(int value);

        void Decrement(int value);

        int PreferedThreadId { get; set; }
    }

    class RequestExecutingContext<TAppSession, TPackageInfo> : IThreadExecutingContext
        where TPackageInfo : IPackageInfo
        where TAppSession : IAppSession, IThreadExecutingContext, IAppSession<TAppSession, TPackageInfo>, new()
    {
        class ExecutingStateCode
        {
            const int NotExecuted = 0;
            const int Executed = 1;
        }

        public TAppSession Session { get; private set; }

        public TPackageInfo RequestInfo { get; private set; }

        int IThreadExecutingContext.PreferedThreadId
        {
            get { return Session.PreferedThreadId; }
            set { Session.PreferedThreadId = value; }
        }

        void IThreadExecutingContext.Increment(int value)
        {
            Session.Increment(value);
        }

        void IThreadExecutingContext.Decrement(int value)
        {
            Session.Decrement(value);
        }

        public bool TryGetExecute()
        {
            return true;
        }

        public RequestExecutingContext()
        {

        }

        public void Initialize(TAppSession session, TPackageInfo requestInfo)
        {
            Session = session;
            RequestInfo = requestInfo;
        }

        public void Reset()
        {
            Session = default(TAppSession);
            RequestInfo = default(TPackageInfo);
        }
    }
}

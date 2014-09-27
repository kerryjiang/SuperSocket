using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;


namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for request executing context
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IRequestExecutingContext<TAppSession, TPackageInfo>
        where TPackageInfo : IPackageInfo
        where TAppSession : IAppSession, IThreadExecutingContext, IAppSession<TAppSession, TPackageInfo>, new()
    {

        /// <summary>
        /// Gets the current session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        TAppSession Session { get; }

        /// <summary>
        /// Gets the current request.
        /// </summary>
        /// <value>
        /// The request information.
        /// </value>
         TPackageInfo RequestInfo { get; }
    }

    /// <summary>
    /// The interface for command excuting context
    /// </summary>
    public interface ICommandExecutingContext
    {
        /// <summary>
        /// Gets the session which is executing the command.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        IAppSession Session { get; }

        /// <summary>
        /// Gets the request which is being processed.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        IPackageInfo Request { get; }


        /// <summary>
        /// Gets the current command which is being executed.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        ICommand CurrentCommand { get;  }


        /// <summary>
        /// Gets the exception which was thrown in the command execution.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        Exception Exception { get; }

        /// <summary>
        /// Gets a value indicating whether [exception handled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exception handled]; otherwise, <c>false</c>.
        /// </value>
        bool ExceptionHandled { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the command exeuction is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        bool Cancel { get; set; }
    }

    /// <summary>
    /// The interface for thread execution context
    /// </summary>
    public interface IThreadExecutingContext
    {
        /// <summary>
        /// Increments the queued request count.
        /// </summary>
        /// <param name="value">The value.</param>
        void Increment(int value);

        /// <summary>
        /// Decrements the queued request count.
        /// </summary>
        /// <param name="value">The value.</param>
        void Decrement(int value);

        /// <summary>
        /// Gets or sets the prefered thread identifier.
        /// </summary>
        /// <value>
        /// The prefered thread identifier.
        /// </value>
        int PreferedThreadId { get; set; }
    }

    class RequestExecutingContext<TAppSession, TPackageInfo> : IRequestExecutingContext<TAppSession, TPackageInfo>, IThreadExecutingContext, ICommandExecutingContext
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
            CurrentCommand = null;
            Exception = null;
            ExceptionHandled = false;
            Cancel = false;
        }

        IAppSession ICommandExecutingContext.Session
        {
            get { return Session; }
        }

        IPackageInfo ICommandExecutingContext.Request
        {
            get { return RequestInfo; }
        }

        /// <summary>
        /// Gets the current command which is being executed.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        public ICommand CurrentCommand { get; internal set; }

        /// <summary>
        /// Gets the exception which was thrown in the command execution.
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
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command exeuction is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }

        private const string m_SlotName = "REC"; //RequestExecutingContext

        public static RequestExecutingContext<TAppSession, TPackageInfo> GetFromThreadContext()
        {
            var slot = Thread.GetNamedDataSlot(m_SlotName);

            // look for existing context instance from thread local at first
            var context = Thread.GetData(slot) as RequestExecutingContext<TAppSession, TPackageInfo>;

            if (context != null)
                return context;

            // create a new context instance and then store it into thread local
            context = new RequestExecutingContext<TAppSession, TPackageInfo>();
            Thread.SetData(slot, context);

            return context;
        }
    }
}

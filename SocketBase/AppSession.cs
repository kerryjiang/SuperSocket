using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// AppSession base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class AppSession<TAppSession, TRequestInfo> : IAppSession, IAppSession<TAppSession, TRequestInfo>
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
        where TRequestInfo : class, IRequestInfo
    {
        #region Properties

        /// <summary>
        /// Gets the app server instance assosiated with the session.
        /// </summary>
        public virtual AppServerBase<TAppSession, TRequestInfo> AppServer { get; private set; }

        /// <summary>
        /// Gets the app server instance assosiated with the session.
        /// </summary>
        IAppServer IAppSession.AppServer
        {
            get { return this.AppServer; }
        }

        /// <summary>
        /// Gets or sets the charset which is used for transfering text message.
        /// </summary>
        /// <value>
        /// The charset.
        /// </value>
        public Encoding Charset { get; set; }

        private IDictionary<object, object> m_Items;

        /// <summary>
        /// Gets the items dictionary, only support 10 items maximum
        /// </summary>
        public IDictionary<object, object> Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new Dictionary<object, object>(10);

                return m_Items;
            }
        }


        private bool m_Connected = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get { return m_Connected; }
            internal set { m_Connected = value; }
        }

        /// <summary>
        /// Gets or sets the previous command.
        /// </summary>
        /// <value>
        /// The prev command.
        /// </value>
        public string PrevCommand { get; set; }

        /// <summary>
        /// Gets or sets the current executing command.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        public string CurrentCommand { get; set; }


        /// <summary>
        /// Gets or sets the secure protocol of transportation layer.
        /// </summary>
        /// <value>
        /// The secure protocol.
        /// </value>
        public SslProtocols SecureProtocol
        {
            get { return SocketSession.SecureProtocol; }
            set { SocketSession.SecureProtocol = value; }
        }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the remote endpoint of client.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILog Logger
        {
            get { return AppServer.Logger; }
        }

        /// <summary>
        /// Gets or sets the last active time of the session.
        /// </summary>
        /// <value>
        /// The last active time.
        /// </value>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }

        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        public ISocketSession SocketSession { get; private set; }

        /// <summary>
        /// Gets the config of the server.
        /// </summary>
        public IServerConfig Config
        {
            get { return AppServer.Config; }
        }

        IReceiveFilter<TRequestInfo> m_ReceiveFilter;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppSession()
        {
            this.StartTime = DateTime.Now;
            this.LastActiveTime = this.StartTime;
        }


        /// <summary>
        /// Initializes the specified app session by AppServer and SocketSession.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        public virtual void Initialize(IAppServer<TAppSession, TRequestInfo> appServer, ISocketSession socketSession)
        {
            var castedAppServer = (AppServerBase<TAppSession, TRequestInfo>)appServer;
            AppServer = castedAppServer;
            Charset = castedAppServer.TextEncoding;
            SocketSession = socketSession;
            SessionID = socketSession.SessionID;
            m_Connected = true;
            m_ReceiveFilter = castedAppServer.ReceiveFilterFactory.CreateFilter(appServer, this, socketSession.RemoteEndPoint);

            var filterInitializer = m_ReceiveFilter as IReceiveFilterInitializer;
            if (filterInitializer != null)
                filterInitializer.Initialize(castedAppServer, this);

            socketSession.Initialize(this);

            OnInit();
        }

        /// <summary>
        /// Starts the session.
        /// </summary>
        void IAppSession.StartSession()
        {
            OnSessionStarted();
        }

        /// <summary>
        /// Called when [init].
        /// </summary>
        protected virtual void OnInit()
        {
            
        }

        /// <summary>
        /// Called when [session started].
        /// </summary>
        protected virtual void OnSessionStarted()
        {

        }

        /// <summary>
        /// Called when [session closed].
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal protected virtual void OnSessionClosed(CloseReason reason)
        {

        }


        /// <summary>
        /// Handles the exceptional error, it only handles application error.
        /// </summary>
        /// <param name="e">The exception.</param>
        protected virtual void HandleException(Exception e)
        {
            Logger.Error(this, e);
            this.Close(CloseReason.ApplicationError);
        }

        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        protected virtual void HandleUnknownRequest(TRequestInfo requestInfo)
        {

        }

        internal void InternalHandleUnknownRequest(TRequestInfo requestInfo)
        {
            HandleUnknownRequest(requestInfo);
        }

        internal void InternalHandleExcetion(Exception e)
        {
            HandleException(e);
        }

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        public virtual void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        #region Sending processing

        /// <summary>
        /// Try to send the message to client.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(string message)
        {
            var data = this.Charset.GetBytes(message);
            return InternalTrySend(new ArraySegment<byte>(data, 0, data.Length));
        }

        /// <summary>
        /// Sends the message to client.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        public virtual void Send(string message)
        {
            var data = this.Charset.GetBytes(message);
            Send(data, 0, data.Length);
        }

        /// <summary>
        /// Try to send the data to client.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(byte[] data, int offset, int length)
        {
            return InternalTrySend(new ArraySegment<byte>(data, offset, length));
        }

        /// <summary>
        /// Sends the data to client.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public virtual void Send(byte[] data, int offset, int length)
        {
            InternalSend(new ArraySegment<byte>(data, offset, length));
        }

        private bool InternalTrySend(ArraySegment<byte> segment)
        {
            if (!SocketSession.TrySend(segment))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Try to send the data segment to client.
        /// </summary>
        /// <param name="segment">The segment which will be sent.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(ArraySegment<byte> segment)
        {
            if (!m_Connected)
                return false;

            return InternalTrySend(segment);
        }


        private void InternalSend(ArraySegment<byte> segment)
        {
            if (!m_Connected)
                return;

            if (InternalTrySend(segment))
                return;

            var sendTimeOut = Config.SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (m_Connected)
            {
                spinWait.SpinOnce();

                if (InternalTrySend(segment))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        /// <summary>
        /// Sends the data segment to client.
        /// </summary>
        /// <param name="segment">The segment which will be sent.</param>
        public virtual void Send(ArraySegment<byte> segment)
        {
            InternalSend(segment);
        }

        private bool InternalTrySend(IList<ArraySegment<byte>> segments)
        {
            if (!SocketSession.TrySend(segments))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Try to send the data segments to client.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue; if it returns false, the sending queue may be full or the socket is not connected</returns>
        public virtual bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (!m_Connected)
                return false;

            return InternalTrySend(segments);
        }

        private void InternalSend(IList<ArraySegment<byte>> segments)
        {
            if (!m_Connected)
                return;

            if (InternalTrySend(segments))
                return;

            var sendTimeOut = Config.SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (m_Connected)
            {
                spinWait.SpinOnce();

                if (InternalTrySend(segments))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        /// <summary>
        /// Sends the data segments to client.
        /// </summary>
        /// <param name="segments">The segments.</param>
        public virtual void Send(IList<ArraySegment<byte>> segments)
        {
            InternalSend(segments);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <param name="paramValues">The parameter values.</param>
        public virtual void Send(string message, params object[] paramValues)
        {
            var data = this.Charset.GetBytes(string.Format(message, paramValues));
            InternalSend(new ArraySegment<byte>(data, 0, data.Length));
        }

        #endregion

        #region Receiving processing

        /// <summary>
        /// Sets the next Receive filter which will be used when next data block received
        /// </summary>
        /// <param name="nextReceiveFilter">The next receive filter.</param>
        protected void SetNextReceiveFilter(IReceiveFilter<TRequestInfo> nextReceiveFilter)
        {
            m_ReceiveFilter = nextReceiveFilter;
        }

        /// <summary>
        /// Filters the request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest, the size of the data which has not been processed</param>
        /// <param name="offsetDelta">return offset delta of next receiving buffer.</param>
        /// <returns></returns>
        TRequestInfo FilterRequest(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest, out int offsetDelta)
        {
            if (!AppServer.OnRawDataReceived(this, readBuffer, offset, length))
            {
                rest = 0;
                offsetDelta = 0;
                return null;
            }

            var currentRequestLength = m_ReceiveFilter.LeftBufferSize;

            var requestInfo = m_ReceiveFilter.Filter(readBuffer, offset, length, toBeCopied, out rest);

            if (m_ReceiveFilter.State == FilterState.Error)
            {
                rest = 0;
                offsetDelta = 0;
                Close(CloseReason.ProtocolError);
                return null;
            }

            var offsetAdapter = m_ReceiveFilter as IOffsetAdapter;

            offsetDelta = offsetAdapter != null ? offsetAdapter.OffsetDelta : 0;

            if (requestInfo == null)
            {
                //current buffered length
                currentRequestLength = m_ReceiveFilter.LeftBufferSize;
            }
            else
            {
                //current request length
                currentRequestLength = currentRequestLength + length - rest;
            }

            if (currentRequestLength >= AppServer.Config.MaxRequestLength)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(this, string.Format("Max request length: {0}, current processed length: {1}", AppServer.Config.MaxRequestLength, currentRequestLength));
                Close(CloseReason.ProtocolError);
                return null;
            }

            //If next Receive filter wasn't set, still use current Receive filter in next round received data processing
            if (m_ReceiveFilter.NextReceiveFilter != null)
                m_ReceiveFilter = m_ReceiveFilter.NextReceiveFilter;

            return requestInfo;
        }

        /// <summary>
        /// Processes the request data.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns>
        /// return offset delta of next receiving buffer
        /// </returns>
        int IAppSession.ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied)
        {
            int rest, offsetDelta;

            while (true)
            {
                var requestInfo = FilterRequest(readBuffer, offset, length, toBeCopied, out rest, out offsetDelta);

                if (requestInfo != null)
                {
                    try
                    {
                        AppServer.ExecuteCommand(this, requestInfo);
                    }
                    catch (Exception e)
                    {
                        HandleException(e);
                    }
                }

                if (rest <= 0)
                {
                    return offsetDelta;
                }

                //Still have data has not been processed
                offset = offset + length - rest;
                length = rest;
            }
        }

        #endregion
    }

    /// <summary>
    /// AppServer basic class for whose request infoe type is StringRequestInfo
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public abstract class AppSession<TAppSession> : AppSession<TAppSession, StringRequestInfo>
        where TAppSession : AppSession<TAppSession, StringRequestInfo>, IAppSession, new()
    {

        private bool m_AppendNewLineForResponse = false;

        private static string m_NewLine = "\r\n";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession&lt;TAppSession&gt;"/> class.
        /// </summary>
        public AppSession()
            : this(true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession&lt;TAppSession&gt;"/> class.
        /// </summary>
        /// <param name="appendNewLineForResponse">if set to <c>true</c> [append new line for response].</param>
        public AppSession(bool appendNewLineForResponse)
        {
            m_AppendNewLineForResponse = appendNewLineForResponse;
        }

        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        protected override void HandleUnknownRequest(StringRequestInfo requestInfo)
        {
            Send("Unknown request: " + requestInfo.Key);
        }

        /// <summary>
        /// Processes the sending message.
        /// </summary>
        /// <param name="rawMessage">The raw message.</param>
        /// <returns></returns>
        protected virtual string ProcessSendingMessage(string rawMessage)
        {
            if (!m_AppendNewLineForResponse)
                return rawMessage;

            if (AppServer.Config.Mode == SocketMode.Udp)
                return rawMessage;

            if (string.IsNullOrEmpty(rawMessage) || !rawMessage.EndsWith(m_NewLine))
                return rawMessage + m_NewLine;
            else
                return rawMessage;
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public override void Send(string message)
        {
            base.Send(ProcessSendingMessage(message));
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="paramValues">The param values.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public override void Send(string message, params object[] paramValues)
        {
            base.Send(ProcessSendingMessage(message), paramValues);
        }
    }

    /// <summary>
    /// AppServer basic class for whose request infoe type is StringRequestInfo
    /// </summary>
    public class AppSession : AppSession<AppSession>
    {

    }
}

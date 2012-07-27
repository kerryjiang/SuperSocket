using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
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
        #region Attributes

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

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected { get; internal set; }

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

        /// <summary>
        /// Gets or sets the m_ request filter.
        /// </summary>
        /// <value>
        /// The m_ request filter.
        /// </value>
        IRequestFilter<TRequestInfo> m_RequestFilter { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppSession()
        {
            this.StartTime = DateTime.Now;
            this.LastActiveTime = this.StartTime;
            this.Charset = Encoding.UTF8;
        }


        /// <summary>
        /// Initializes the specified app session by AppServer and SocketSession.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <param name="requestFilter">The request filter.</param>
        public virtual void Initialize(IAppServer<TAppSession, TRequestInfo> appServer, ISocketSession socketSession, IRequestFilter<TRequestInfo> requestFilter)
        {
            AppServer = (AppServerBase<TAppSession, TRequestInfo>)appServer;
            SocketSession = socketSession;
            SessionID = socketSession.SessionID;
            Connected = true;
            m_RequestFilter = requestFilter;
            OnInit();
        }

        /// <summary>
        /// Starts the session.
        /// </summary>
        public void StartSession()
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
        /// Handles the exceptional error.
        /// </summary>
        /// <param name="e">The exception.</param>
        public virtual void HandleException(Exception e)
        {
            Logger.Error(this, e);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cmdInfo">The CMD info.</param>
        public void ExecuteCommand(TAppSession session, TRequestInfo cmdInfo)
        {
            AppServer.ExecuteCommand(session, cmdInfo);
        }

        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        public virtual void HandleUnknownRequest(TRequestInfo requestInfo)
        {
            SendResponse("Unknown request: " + requestInfo.Key);
        }

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        public virtual void Close(CloseReason reason)
        {
            try
            {
                this.SocketSession.Close(reason);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        #region sending processing

        private IBatchQueue<ArraySegment<byte>> m_SendingQueue = new ConcurrentBatchQueue<ArraySegment<byte>>();

        /// <summary>
        /// Tries to get the data segment to be sent.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>
        /// return whether has data to send
        /// </returns>
        bool IAppSession.TryGetSendingData(IList<ArraySegment<byte>> segments)
        {
            return m_SendingQueue.TryDequeue(segments);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        public virtual void SendResponse(string message)
        {
            var data = this.Charset.GetBytes(message);
            SendResponse(data, 0, data.Length);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public virtual void SendResponse(byte[] data, int offset, int length)
        {
            SendResponse(new ArraySegment<byte>(data, offset, length));
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="segment">The segment which will be sent.</param>
        public virtual void SendResponse(ArraySegment<byte> segment)
        {
            m_SendingQueue.Enqueue(segment);
            SocketSession.StartSend();
            LastActiveTime = DateTime.Now;
        }


        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="segments">The segments.</param>
        public virtual void SendResponse(IList<ArraySegment<byte>> segments)
        {
            m_SendingQueue.Enqueue(segments);
            SocketSession.StartSend();
            LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <param name="paramValues">The parameter values.</param>
        public virtual void SendResponse(string message, params object[] paramValues)
        {
            SendResponse(string.Format(message, paramValues));
        }

        #endregion

        #region receiving processing

        /// <summary>
        /// Sets the next request filter which will be used when next data block received
        /// </summary>
        /// <param name="nextRequestFilter">The next request filter.</param>
        protected void SetNextRequestFilter(IRequestFilter<TRequestInfo> nextRequestFilter)
        {
            m_RequestFilter = nextRequestFilter;
        }

        /// <summary>
        /// Filters the request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left, the size of the data which has not been processed</param>
        /// <param name="offsetDelta">return offset delta of next receiving buffer.</param>
        /// <returns></returns>
        TRequestInfo FilterRequest(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left, out int offsetDelta)
        {
            if (!AppServer.OnRawDataReceived(this, readBuffer, offset, length))
            {
                left = 0;
                offsetDelta = 0;
                return null;
            }

            var requestInfo = m_RequestFilter.Filter(this, readBuffer, offset, length, toBeCopied, out left);

            var offsetAdapter = m_RequestFilter as IOffsetAdapter;

            offsetDelta = offsetAdapter != null ? offsetAdapter.OffsetDelta : 0;

            if (requestInfo == null)
            {
                int leftBufferCount = m_RequestFilter.LeftBufferSize;
                if (leftBufferCount >= AppServer.Config.MaxRequestLength)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.ErrorFormat("Max request length: {0}, current processed length: {1}", AppServer.Config.MaxRequestLength, leftBufferCount);
                    Close(CloseReason.ServerClosing);
                    return null;
                }
            }

            //If next request filter wasn't set, still use current request filter in next round received data processing
            if (m_RequestFilter.NextRequestFilter != null)
                m_RequestFilter = m_RequestFilter.NextRequestFilter;

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
            int left, offsetDelta;

            while (true)
            {
                var requestInfo = FilterRequest(readBuffer, offset, length, toBeCopied, out left, out offsetDelta);

                if (requestInfo == null)
                    return offsetDelta;

                AppServer.ExecuteCommand(this, requestInfo);

                if (left <= 0)
                    return offsetDelta;

                offset = offset + length - left;
                length = left;

                continue;
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

            if (string.IsNullOrEmpty(rawMessage) || !rawMessage.EndsWith(Environment.NewLine))
                return rawMessage + Environment.NewLine;
            else
                return rawMessage;
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendResponse(string message)
        {
            base.SendResponse(ProcessSendingMessage(message));
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="paramValues">The param values.</param>
        public override void SendResponse(string message, params object[] paramValues)
        {
            base.SendResponse(ProcessSendingMessage(message), paramValues);
        }
    }

    /// <summary>
    /// AppServer basic class for whose request infoe type is StringRequestInfo
    /// </summary>
    public class AppSession : AppSession<AppSession>
    {

    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Protocol;
using SuperSocket.WebSocket.SubProtocol;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocketSession basic interface
    /// </summary>
    public interface IWebSocketSession : IAppSession
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        string Method { get; set; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        string HttpVersion { get; set; }

        /// <summary>
        /// Gets the sec web socket version.
        /// </summary>
        string SecWebSocketVersion { get; }

        /// <summary>
        /// Gets the origin.
        /// </summary>
        string Origin { get; }

        /// <summary>
        /// Gets the URI scheme.
        /// </summary>
        string UriScheme { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IWebSocketSession" /> is handshaked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if handshaked; otherwise, <c>false</c>.
        /// </value>
        bool Handshaked { get; }

        /// <summary>
        /// Sends the raw binary response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void SendRawData(byte[] data, int offset, int length);

        /// <summary>
        /// Try to send the raw binary response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>if the data to be sent is queued, return true, else the queue is full, then return false</returns>
        bool TrySendRawData(byte[] data, int offset, int length);

        /// <summary>
        /// Gets the app server.
        /// </summary>
        new IWebSocketServer AppServer { get; }

        /// <summary>
        /// Gets or sets the protocol processor.
        /// </summary>
        /// <value>
        /// The protocol processor.
        /// </value>
        IProtocolProcessor ProtocolProcessor { get; set; }

        /// <summary>
        /// Gets the available sub protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns></returns>
        string GetAvailableSubProtocol(string protocol);
    }

    /// <summary>
    /// WebSocket AppSession
    /// </summary>
    public class WebSocketSession : WebSocketSession<WebSocketSession>
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        public new WebSocketServer AppServer
        {
            get { return (WebSocketServer)base.AppServer; }
        }
    }

    /// <summary>
    /// WebSocket AppSession class
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public class WebSocketSession<TWebSocketSession> : AppSession<TWebSocketSession, IWebSocketFragment>, IWebSocketSession, IAppSession
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        /// <value>
        /// The HTTP version.
        /// </value>
        public string HttpVersion { get; set; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        public string Host { get { return this.Items.GetValue<string>(WebSocketConstant.Host, string.Empty); } }

        /// <summary>
        /// Gets the origin.
        /// </summary>
        public string Origin { get; internal set; }

        /// <summary>
        /// Gets the upgrade.
        /// </summary>
        public string Upgrade { get { return this.Items.GetValue<string>(WebSocketConstant.Upgrade, string.Empty); } }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public string Connection { get { return this.Items.GetValue<string>(WebSocketConstant.Connection, string.Empty); } }

        /// <summary>
        /// Gets the sec web socket version.
        /// </summary>
        public string SecWebSocketVersion { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketVersion, string.Empty); } }

        /// <summary>
        /// Gets the sec web socket protocol.
        /// </summary>
        public string SecWebSocketProtocol { get { return this.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty); } }

        internal List<WebSocketDataFrame> Frames { get; private set; }

        internal DateTime StartClosingHandshakeTime { get; private set; }

        private const string m_CurrentTokenSlotName = "CurrentRequestToken";

        internal LocalDataStoreSlot SetCurrentToken(string token)
        {
            var slot = Thread.GetNamedDataSlot(m_CurrentTokenSlotName);
            Thread.SetData(slot, token);
            return slot;
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public string CurrentToken
        {
            get
            {
                return Thread.GetData(Thread.GetNamedDataSlot(m_CurrentTokenSlotName)) as string;
            }
        }

        /// <summary>
        /// Gets the app server.
        /// </summary>
        public new WebSocketServer<TWebSocketSession> AppServer
        {
            get { return (WebSocketServer<TWebSocketSession>)base.AppServer; }
        }

        IWebSocketServer IWebSocketSession.AppServer
        {
            get { return (IWebSocketServer)base.AppServer; }
        }

        string IWebSocketSession.GetAvailableSubProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                SubProtocol = AppServer.DefaultSubProtocol;
                return string.Empty;
            }

            var arrNames = protocol.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var name in arrNames)
            {
                var subProtocol = AppServer.GetSubProtocol(name);

                if (subProtocol != null)
                {
                    SubProtocol = subProtocol;
                    return name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the URI scheme, ws or wss
        /// </summary>
        public string UriScheme
        {
            get
            {
                if (SocketSession.SecureProtocol == SslProtocols.None)
                    return WebSocketConstant.WsSchema;
                else
                    return WebSocketConstant.WssSchema;
            }
        }

        /// <summary>
        /// Gets the sub protocol.
        /// </summary>
        public ISubProtocol<TWebSocketSession> SubProtocol { get; private set; }

        private bool m_Handshaked = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="IWebSocketSession" /> is handshaked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if handshaked; otherwise, <c>false</c>.
        /// </value>
        public bool Handshaked
        {
            get { return m_Handshaked; }
        }

        internal void OnHandshakeSuccess()
        {
            m_Handshaked = true;
            SetCookie();
            OnSessionStarted();
            AppServer.FireOnNewSessionConnected(this);
        }

        /// <summary>
        /// Gets a value indicating whether the session [in closing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in closing]; otherwise, <c>false</c>.
        /// </value>
        public bool InClosing { get; private set; }

        /// <summary>
        /// Called when [init].
        /// </summary>
        protected override void OnInit()
        {
            Frames = new List<WebSocketDataFrame>();
            base.OnInit();
        }

        void IAppSession.StartSession()
        {
            //Do nothing. Avoid firing thhe OnSessionStarted() method of base class
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        private void SetCookie()
        {
            string cookieValue = this.Items.GetValue<string>(WebSocketConstant.Cookie, string.Empty);

            if (string.IsNullOrEmpty(cookieValue))
                return;

            var cookies = new StringDictionary();
            this.Cookies = cookies;

            string[] pairs = cookieValue.Split(';');

            int pos;
            string key, value;

            foreach (var p in pairs)
            {
                pos = p.IndexOf('=');

                if (pos <= 0)
                    continue;

                key = p.Substring(0, pos).Trim();

                pos += 1;

                if (pos < p.Length)
                    value = p.Substring(pos).Trim();
                else
                    value = string.Empty;

                if (string.IsNullOrEmpty(value))
                {
                    cookies[key] = string.Empty;
                    continue;
                }

                try
                {
                    cookies[key] = Uri.UnescapeDataString(value);
                }
                catch (Exception e)
                {
                    Logger.Error(this, string.Format("Failed to read cookie, key: {0}, value: {1}.", key, value), e);
                }
            }
        }


        /// <summary>
        /// Gets the cookies.
        /// </summary>
        public StringDictionary Cookies { get; private set; }


        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Send(string message)
        {
            ProtocolProcessor.SendMessage(this, message);
        }

        /// <summary>
        /// Tries to send.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns></returns>
        public override bool TrySend(string message)
        {
            return ProtocolProcessor.TrySendMessage(this, message);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public override void Send(byte[] data, int offset, int length)
        {
            if (!ProtocolProcessor.CanSendBinaryData)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("The websocket of this version cannot used for sending binary data!");
                return;
            }

            ProtocolProcessor.SendData(this, data, offset, length);
        }

        /// <summary>
        /// Tries to send the data over the websocket connection.
        /// </summary>
        /// <param name="segment">The segment to be sent.</param>
        /// <returns></returns>
        public override bool TrySend(ArraySegment<byte> segment)
        {
            if (!ProtocolProcessor.CanSendBinaryData)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("The websocket of this version cannot used for sending binary data!");
                return false;
            }

            return ProtocolProcessor.TrySendData(this, segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Tries to send the data over the websocket connection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public override bool TrySend(byte[] data, int offset, int length)
        {
            if (!ProtocolProcessor.CanSendBinaryData)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("The websocket of this version cannot used for sending binary data!");
                return false;
            }

            return ProtocolProcessor.TrySendData(this, data, offset, length);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public override void Send(ArraySegment<byte> segment)
        {
            this.Send(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Sends the raw binary data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void IWebSocketSession.SendRawData(byte[] data, int offset, int length)
        {
            base.Send(data, offset, length);
        }


        /// <summary>
        /// Try to send the raw binary response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// if the data to be sent is queued, return true, else the queue is full, then return false
        /// </returns>
        bool IWebSocketSession.TrySendRawData(byte[] data, int offset, int length)
        {
            return base.TrySend(new ArraySegment<byte>(data, offset, length));
        }

        /// <summary>
        /// Closes the with handshake.
        /// </summary>
        /// <param name="reasonText">The reason text.</param>
        public void CloseWithHandshake(string reasonText)
        {
            this.CloseWithHandshake(ProtocolProcessor.CloseStatusClode.NormalClosure, reasonText);
        }

        /// <summary>
        /// Closes the with handshake.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonText">The reason text.</param>
        public void CloseWithHandshake(int statusCode, string reasonText)
        {
            if (!InClosing)
                InClosing = true;

            ProtocolProcessor.SendCloseHandshake(this, statusCode, reasonText);

            StartClosingHandshakeTime = DateTime.Now;
            AppServer.PushToCloseHandshakeQueue(this);
        }

        /// <summary>
        /// Sends the close handshake response.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public void SendCloseHandshakeResponse(int statusCode)
        {
            if (!InClosing)
                InClosing = true;

            ProtocolProcessor.SendCloseHandshake(this, statusCode, string.Empty);
        }

        /// <summary>
        /// Closes the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public override void Close(CloseReason reason)
        {
            if (reason == CloseReason.TimeOut && ProtocolProcessor != null)
            {
                CloseWithHandshake(ProtocolProcessor.CloseStatusClode.NormalClosure, "Session timeOut");
                return;
            }

            base.Close(reason);
        }

        /// <summary>
        /// Gets or sets the protocol processor.
        /// </summary>
        /// <value>
        /// The protocol processor.
        /// </value>
        public IProtocolProcessor ProtocolProcessor { get; set; }

        /// <summary>
        /// Handles the unknown command.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        internal protected virtual void HandleUnknownCommand(SubRequestInfo requestInfo)
        {

        }

        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        protected override void HandleUnknownRequest(IWebSocketFragment requestInfo)
        {
            base.Close();
        }
    }
}

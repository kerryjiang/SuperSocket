using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Command;
using SuperSocket.WebSocket.Config;
using SuperSocket.WebSocket.Protocol;
using SuperSocket.WebSocket.SubProtocol;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket server interface
    /// </summary>
    public interface IWebSocketServer : IAppServer
    {
        /// <summary>
        /// Gets the web socket protocol processor.
        /// </summary>
        IProtocolProcessor WebSocketProtocolProcessor { get; }

        /// <summary>
        /// Validates the handshake request.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="origin">The origin.</param>
        /// <returns>
        /// the validation result
        /// </returns>
        bool ValidateHandshake(IWebSocketSession session, string origin);
    }

    /// <summary>
    /// WebSocket AppServer
    /// </summary>
    public class WebSocketServer : WebSocketServer<WebSocketSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="subProtocols">The sub protocols.</param>
        public WebSocketServer(IEnumerable<ISubProtocol<WebSocketSession>> subProtocols)
            : base(subProtocols)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="subProtocol">The sub protocol.</param>
        public WebSocketServer(ISubProtocol<WebSocketSession> subProtocol)
            : base(subProtocol)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        public WebSocketServer()
            : base(new List<ISubProtocol<WebSocketSession>>())
        {

        }
    }

    /// <summary>
    /// WebSocket AppServer
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public abstract class WebSocketServer<TWebSocketSession> : AppServer<TWebSocketSession, IWebSocketFragment>, IWebSocketServer
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private IBinaryDataConverter m_BinaryDataConverter;

        /// <summary>
        /// Gets or sets the binary data converter.
        /// </summary>
        /// <value>
        /// The binary data converter.
        /// </value>
        protected IBinaryDataConverter BinaryDataConverter
        {
            get { return m_BinaryDataConverter; }
            set { m_BinaryDataConverter = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        /// <param name="subProtocols">The sub protocols.</param>
        public WebSocketServer(IEnumerable<ISubProtocol<TWebSocketSession>> subProtocols)
            : this()
        {
            if (!subProtocols.Any())
                return;

            foreach (var protocol in subProtocols)
            {
                if (!RegisterSubProtocol(protocol))
                    throw new Exception("Failed to register sub protocol!");
            }

            m_SubProtocolConfigured = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        /// <param name="subProtocol">The sub protocol.</param>
        public WebSocketServer(ISubProtocol<TWebSocketSession> subProtocol)
            : this(new List<ISubProtocol<TWebSocketSession>> { subProtocol })
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        public WebSocketServer()
            : base(new WebSocketProtocol())
        {

        }

        private Dictionary<string, ISubProtocol<TWebSocketSession>> m_SubProtocols = new Dictionary<string, ISubProtocol<TWebSocketSession>>(StringComparer.OrdinalIgnoreCase);

        internal ISubProtocol<TWebSocketSession> DefaultSubProtocol { get; private set; }

        private bool m_SubProtocolConfigured = false;

        private ConcurrentQueue<TWebSocketSession> m_OpenHandshakePendingQueue = new ConcurrentQueue<TWebSocketSession>();

        private ConcurrentQueue<TWebSocketSession> m_CloseHandshakePendingQueue = new ConcurrentQueue<TWebSocketSession>();

        /// <summary>
        /// The openning handshake timeout, in seconds
        /// </summary>
        private int m_OpenHandshakeTimeOut;

        /// <summary>
        /// The closing handshake timeout, in seconds
        /// </summary>
        private int m_CloseHandshakeTimeOut;

        /// <summary>
        /// The interval of checking handshake pending queue, in seconds
        /// </summary>
        private int m_HandshakePendingQueueCheckingInterval;


        private Timer m_HandshakePendingQueueCheckingTimer;

        /// <summary>
        /// Gets the sub protocol by sub protocol name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal ISubProtocol<TWebSocketSession> GetSubProtocol(string name)
        {
            ISubProtocol<TWebSocketSession> subProtocol;

            if (m_SubProtocols.TryGetValue(name, out subProtocol))
                return subProtocol;
            else
                return null;
        }

        private IProtocolProcessor m_WebSocketProtocolProcessor;

        IProtocolProcessor IWebSocketServer.WebSocketProtocolProcessor
        {
            get { return m_WebSocketProtocolProcessor; }
        }

        /// <summary>
        /// Gets the request filter factory.
        /// </summary>
        public new WebSocketProtocol ReceiveFilterFactory
        {
            get
            {
                return (WebSocketProtocol)base.ReceiveFilterFactory;
            }
        }

        bool IWebSocketServer.ValidateHandshake(IWebSocketSession session, string origin)
        {
            var s = (TWebSocketSession)session;
            s.Origin = origin;
            return ValidateHandshake(s, origin);
        }

        /// <summary>
        /// Validates the handshake request.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="origin">The origin in the handshake request.</param>
        /// <returns></returns>
        protected virtual bool ValidateHandshake(TWebSocketSession session, string origin)
        {
            return true;
        }

        bool RegisterSubProtocol(ISubProtocol<TWebSocketSession> subProtocol)
        {
            if (m_SubProtocols.ContainsKey(subProtocol.Name))
            {
                if(Logger.IsErrorEnabled)
                    Logger.ErrorFormat("Cannot register duplicate name sub protocol! Duplicate name: {0}.", subProtocol.Name);
                return false;
            }

            m_SubProtocols.Add(subProtocol.Name, subProtocol);
            return true;
        }

        private bool SetupSubProtocols(IServerConfig config)
        {
            //Preparing sub protocols' configuration
            var subProtocolConfigSection = config.GetChildConfig<SubProtocolConfigCollection>("subProtocols");

            var subProtocolConfigDict = new Dictionary<string, SubProtocolConfig>(subProtocolConfigSection == null ? 0 : subProtocolConfigSection.Count, StringComparer.OrdinalIgnoreCase);

            if (subProtocolConfigSection != null)
            {
                foreach (var protocolConfig in subProtocolConfigSection)
                {
                    string originalProtocolName = protocolConfig.Name;
                    string protocolName;
                    
                    ISubProtocol<TWebSocketSession> subProtocolInstance;

                    if (!string.IsNullOrEmpty(originalProtocolName))
                    {
                        protocolName = originalProtocolName;

                        if (!string.IsNullOrEmpty(protocolConfig.Type))
                        {
                            try
                            {
                                subProtocolInstance = AssemblyUtil.CreateInstance<ISubProtocol<TWebSocketSession>>(protocolConfig.Type, new object[] { originalProtocolName });
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                return false;
                            }

                            if (!RegisterSubProtocol(subProtocolInstance))
                                return false;
                        }
                        else
                        {
                            if (!m_SubProtocols.ContainsKey(protocolName))
                            {
                                subProtocolInstance = new BasicSubProtocol<TWebSocketSession>(protocolName);

                                if (!RegisterSubProtocol(subProtocolInstance))
                                    return false;
                            }
                        }
                    }
                    else
                    {
                        protocolName = BasicSubProtocol<TWebSocketSession>.DefaultName;

                        if (!string.IsNullOrEmpty(protocolConfig.Type))
                        {
                            if(Logger.IsErrorEnabled)
                                Logger.Error("You needn't set Type attribute for SubProtocol, if you don't set Name attribute!");
                            return false;
                        }
                    }

                    subProtocolConfigDict[protocolName] = protocolConfig;
                }

                if(subProtocolConfigDict.Values.Any())
                    m_SubProtocolConfigured = true;
            }

            if (m_SubProtocols.Count <= 0 || (subProtocolConfigDict.ContainsKey(BasicSubProtocol<TWebSocketSession>.DefaultName) && !m_SubProtocols.ContainsKey(BasicSubProtocol<TWebSocketSession>.DefaultName)))
            {
                if (!RegisterSubProtocol(BasicSubProtocol<TWebSocketSession>.CreateDefaultSubProtocol()))
                    return false;
            }

            //Initialize sub protocols
            foreach (var subProtocol in m_SubProtocols.Values)
            {
                SubProtocolConfig protocolConfig = null;

                subProtocolConfigDict.TryGetValue(subProtocol.Name, out protocolConfig);

                bool initialized = false;

                try
                {
                    initialized = subProtocol.Initialize(this, protocolConfig, Logger);
                }
                catch (Exception e)
                {
                    initialized = false;
                    Logger.Error(e);
                }

                if (!initialized)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.ErrorFormat("Failed to initialize the sub protocol '{0}'!", subProtocol.Name);
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Setups with the specified root config.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            if (m_SubProtocols != null && m_SubProtocols.Count > 0)
                DefaultSubProtocol = m_SubProtocols.Values.FirstOrDefault();

            m_WebSocketProtocolProcessor = new DraftHybi10Processor
            {
                NextProcessor = new Rfc6455Processor
                {
                    NextProcessor = new DraftHybi00Processor()
                }
            };

            SetupMultipleProtocolSwitch(m_WebSocketProtocolProcessor);

            if (!int.TryParse(config.Options.GetValue("handshakePendingQueueCheckingInterval"), out m_HandshakePendingQueueCheckingInterval))
                m_HandshakePendingQueueCheckingInterval = 60;// 1 minute default


            if (!int.TryParse(config.Options.GetValue("openHandshakeTimeOut"), out m_OpenHandshakeTimeOut))
                m_OpenHandshakeTimeOut = 120;// 2 minute default

            if (!int.TryParse(config.Options.GetValue("closeHandshakeTimeOut"), out m_CloseHandshakeTimeOut))
                m_CloseHandshakeTimeOut = 120;// 2 minute default

            if (m_BinaryDataConverter == null)
            {
                m_BinaryDataConverter = new TextEncodingBinaryDataConverter(Encoding.UTF8);
            }

            return true;
        }

        private void SetupMultipleProtocolSwitch(IProtocolProcessor rootProcessor)
        {
            var thisProcessor = rootProcessor;

            List<int> availableVersions = new List<int>();

            while (true)
            {
                if (thisProcessor.Version > 0)
                    availableVersions.Add(thisProcessor.Version);

                if (thisProcessor.NextProcessor == null)
                    break;

                thisProcessor = thisProcessor.NextProcessor;
            }

            thisProcessor.NextProcessor = new MultipleProtocolSwitchProcessor(availableVersions.ToArray());
        }

        /// <summary>
        /// Called when [started].
        /// </summary>
        protected override void OnStarted()
        {
            m_HandshakePendingQueueCheckingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
            base.OnStarted();
        }

        private void HandshakePendingQueueCheckingCallback(object state)
        {
            try
            {
                m_HandshakePendingQueueCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                while (true)
                {
                    TWebSocketSession session;

                    if (!m_OpenHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (session.Handshaked || !session.Connected)
                    {
                        //Handshaked or not connected
                        m_OpenHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartTime.AddSeconds(m_OpenHandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    m_OpenHandshakePendingQueue.TryDequeue(out session);
                    session.Close(CloseReason.TimeOut);
                }

                while (true)
                {
                    TWebSocketSession session;

                    if (!m_CloseHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (!session.Connected)
                    {
                        //the session has been closed
                        m_CloseHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartClosingHandshakeTime.AddSeconds(m_CloseHandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    m_CloseHandshakePendingQueue.TryDequeue(out session);
                    //Needn't send closing handshake again
                    session.Close(CloseReason.ServerClosing);
                }
            }
            catch (Exception e)
            {
                if(Logger.IsErrorEnabled)
                    Logger.Error(e);
            }
            finally
            {
                m_HandshakePendingQueueCheckingTimer.Change(m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
            }
        }

        internal void PushToCloseHandshakeQueue(IAppSession appSession)
        {
            m_CloseHandshakePendingQueue.Enqueue((TWebSocketSession)appSession);
        }

        /// <summary>
        /// Called when [new session connected].
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void OnNewSessionConnected(TWebSocketSession session)
        {
            m_OpenHandshakePendingQueue.Enqueue(session);
        }

        internal void FireOnNewSessionConnected(IAppSession appSession)
        {
            base.OnNewSessionConnected((TWebSocketSession)appSession);
        }

        /// <summary>
        /// Occurs when [new request received].
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override event RequestHandler<TWebSocketSession,IWebSocketFragment> NewRequestReceived
        {
            add { throw new NotSupportedException("Please use NewMessageReceived instead!"); }
            remove { throw new NotSupportedException("Please use NewMessageReceived instead!"); }
        }

        private SessionHandler<TWebSocketSession, string> m_NewMessageReceived;

        /// <summary>
        /// Occurs when [new message received].
        /// </summary>
        public event SessionHandler<TWebSocketSession, string> NewMessageReceived
        {
            add
            {
                if (m_SubProtocolConfigured)
                    throw new Exception("If you have defined any sub protocol, you cannot subscribe NewMessageReceived event!");

                m_NewMessageReceived += value;
            }
            remove
            {
                m_NewMessageReceived -= value;
            }
        }

        void ExecuteMessage(TWebSocketSession session, string message)
        {
            if (session.SubProtocol == null)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("No SubProtocol selected! This session cannot process any message!");
                session.CloseWithHandshake(session.ProtocolProcessor.CloseStatusClode.ProtocolError, "No SubProtocol selected");
                return;
            }

            ExecuteSubCommand(session, session.SubProtocol.SubRequestParser.ParseRequestInfo(message));
        }

        internal void OnNewMessageReceived(TWebSocketSession session, string message)
        {
            if (m_NewMessageReceived == null)
            {
                ExecuteMessage(session, message);
            }
            else
            {
                m_NewMessageReceived(session, message);
            }
        }

        private SessionHandler<TWebSocketSession, byte[]> m_NewDataReceived;

        /// <summary>
        /// Occurs when [new data received].
        /// </summary>
        public event SessionHandler<TWebSocketSession, byte[]> NewDataReceived
        {
            add
            {
                m_NewDataReceived += value;
            }
            remove
            {
                m_NewDataReceived -= value;
            }
        }

        internal void OnNewDataReceived(TWebSocketSession session, byte[] data)
        {
            if (m_NewDataReceived == null)
            {
                var converter = m_BinaryDataConverter;

                if (converter != null)
                {
                    ExecuteMessage(session, converter.ToString(data, 0, data.Length));
                }

                return;
            }

            m_NewDataReceived(session, data);
        }

        private const string m_Tab = "\t";
        private const char m_Colon = ':';
        private const string m_Space = " ";
        private const char m_SpaceChar = ' ';
        private const string m_ValueSeparator = ", ";

        internal static void ParseHandshake(IWebSocketSession session, TextReader reader)
        {
            string line;
            string firstLine = string.Empty;
            string prevKey = string.Empty;

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = line;
                    continue;
                }

                if (line.StartsWith(m_Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    string currentValue = session.Items.GetValue<string>(prevKey, string.Empty);
                    session.Items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(m_Colon);

                if (pos <= 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                var valueOffset = pos + 1;

                if (line.Length <= valueOffset) //No value in this line
                    continue;

                string value = line.Substring(valueOffset);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(m_Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                object oldValue;

                if (!session.Items.TryGetValue(key, out oldValue))
                {
                    session.Items.Add(key, value);
                }
                else
                {
                    session.Items[key] = oldValue + m_ValueSeparator + value;
                }

                prevKey = key;
            }

            var metaInfo = firstLine.Split(m_SpaceChar);

            session.Method = metaInfo[0];
            session.Path = metaInfo[1];
            session.HttpVersion = metaInfo[2];
        }

        /// <summary>
        /// Setups the commands.
        /// </summary>
        /// <param name="discoveredCommands">The discovered commands.</param>
        /// <returns></returns>
        protected override bool SetupCommands(Dictionary<string, ICommand<TWebSocketSession, IWebSocketFragment>> discoveredCommands)
        {
            var commands = new List<ICommand<TWebSocketSession, IWebSocketFragment>>
                {
                    new HandShake<TWebSocketSession>(),
                    new Text<TWebSocketSession>(),  
                    new Binary<TWebSocketSession>(),
                    new Close<TWebSocketSession>(),
                    new Ping<TWebSocketSession>(),
                    new Pong<TWebSocketSession>(),
                    new Continuation<TWebSocketSession>(),
                    new Plain<TWebSocketSession>()
                };

            commands.ForEach(c => discoveredCommands.Add(c.Name, c));

            if (!SetupSubProtocols(Config))
                return false;

            return true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        protected override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
        {
            if (session.InClosing)
            {
                //Only handle closing handshake if the session is in closing
                if (requestInfo.Key != OpCode.CloseTag)
                    return;
            }

            base.ExecuteCommand(session, requestInfo);
        }

        private void ExecuteSubCommand(TWebSocketSession session, SubRequestInfo requestInfo)
        {
            ISubCommand<TWebSocketSession> subCommand;

            if (session.SubProtocol.TryGetCommand(requestInfo.Key, out subCommand))
            {
                session.CurrentCommand = requestInfo.Key;
                subCommand.ExecuteCommand(session, requestInfo);
                session.PrevCommand = requestInfo.Key;

                if (Config.LogCommand && Logger.IsInfoEnabled)
                    Logger.Info(session, string.Format("Command - {0} - {1}", session.SessionID, requestInfo.Key));
            }
            else
            {
                session.HandleUnknownCommand(requestInfo);
            }

            session.LastActiveTime = DateTime.Now;
        }

        #region JSON serialize/deserialize

        /// <summary>
        /// Serialize the target object by JSON
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public virtual string JsonSerialize(object target)
        {
            return JsonConvert.SerializeObject(target);
        }

        /// <summary>
        /// Deserialize the JSON string to target type object.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual object JsonDeserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        #endregion
    }
}

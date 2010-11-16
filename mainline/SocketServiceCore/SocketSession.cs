using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract class SocketSession<T> : ISocketSession<T>
        where T : IAppSession, new()
    {
        public IAppServer<T> AppServer { get; private set; }

        public T AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        public SocketSession()
        {

        }

        public virtual void Initialize(IAppServer<T> appServer, T appSession, Socket client)
        {
            AppServer = appServer;
            AppSession = appSession;
            Client = client;
        }

        /// <summary>
        /// The session identity string
        /// </summary>
        private string m_SessionID = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID
        {
            get { return m_SessionID; }
        }


        private string m_IdentityKey;
        /// <summary>
        /// Gets or sets the IdentityKey, in some case we cannot use sessionID as key directly
        /// Then we need to use indentity key
        /// </summary>
        /// <value>The IdentityKey.</value>
        public string IdentityKey
        {
            get
            {
                if (string.IsNullOrEmpty(m_IdentityKey))
                    return m_SessionID;

                return m_IdentityKey;
            }

            protected set
            {
                m_IdentityKey = value;
            }
        }

        private DateTime m_StartTime = DateTime.Now;

        /// <summary>
        /// Gets the session start time.
        /// </summary>
        /// <value>The session start time.</value>
        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        private DateTime m_LastActiveTime = DateTime.Now;

        /// <summary>
        /// Gets the last active time of the session.
        /// </summary>
        /// <value>The last active time.</value>
        public DateTime LastActiveTime
        {
            get { return m_LastActiveTime; }
            protected set { m_LastActiveTime = value; }
        }

        private IServerConfig m_Config = null;

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>The config.</value>
        public IServerConfig Config
        {
            get { return m_Config; }
            set { m_Config = value; }
        }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public void Start()
        {
            Start(AppSession.Context);
        }

        protected abstract void Start(SocketContext context);


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cmdInfo">The CMD info.</param>
        protected virtual void ExecuteCommand(string commandLine)
        {
            CommandInfo cmdInfo = AppServer.CommandParser.ParseCommand(commandLine);
            ICommand<T> command = AppServer.GetCommandByName(cmdInfo.Name);

            if (command != null)
            {
                AppSession.Context.CurrentCommand = cmdInfo.Name;
                command.ExecuteCommand(AppSession, cmdInfo);
                AppSession.Context.PrevCommand = cmdInfo.Name;
                if (AppServer.Config.LogCommand)
                    LogUtil.LogInfo(AppServer, string.Format("Command - {0} - {1}", IdentityKey, cmdInfo.Name));
            }
            else
            {
                AppSession.HandleUnknownCommand(cmdInfo);
            }

            LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void SayWelcome()
        {
            AppSession.SayWelcome();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose()
        {
            var closedHandler = Closed;
            if (closedHandler != null)
                closedHandler(this, new SocketSessionClosedEventArgs { IdentityKey = this.IdentityKey });
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public event EventHandler<SocketSessionClosedEventArgs> Closed;

        protected virtual void HandleExceptionalError(Exception e)
        {
            AppSession.HandleExceptionalError(e);
        }

        public abstract void SendResponse(SocketContext context, string message);

        public abstract void SendResponse(SocketContext context, byte[] data);

        public abstract void ApplySecureProtocol(SocketContext context);

        public abstract void ReceiveData(Stream storeSteram, int length);

        public abstract void ReceiveData(Stream storeSteram, byte[] endMark);

        public Stream GetUnderlyStream()
        {
            return new NetworkStream(Client);
        }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public Socket Client { get; set; }

        private bool m_IsClosed = false;

        protected bool IsClosed
        {
            get { return m_IsClosed; }
        }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>The local end point.</value>
        public virtual IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)Client.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public virtual IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)Client.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        /// <summary>
        /// Closes this connection.
        /// </summary>
        public virtual void Close()
        {
            if (Client == null && m_IsClosed)
                return;

            lock (SyncRoot)
            {
                if (Client == null && m_IsClosed)
                    return;

                try
                {
                    if (Client != null)
                        Client.Shutdown(SocketShutdown.Both);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, e);
                }

                try
                {
                    if (Client != null)
                        Client.Close();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, e);
                }
                finally
                {
                    Client = null;
                    m_IsClosed = true;
                    OnClose();
                }
            }
        }

        protected bool DetectEndMark(byte[] buffer, int thisRead, byte[] endMark, byte[] lastData, ref int lastDataSzie)
        {
            if (thisRead >= endMark.Length)
            {
                if (buffer.EndsWith(0, thisRead, endMark))
                    return true;

                Array.Copy(buffer, thisRead - endMark.Length - 1, lastData, 0, endMark.Length);
                lastDataSzie = endMark.Length;
            }
            else
            {
                if (lastDataSzie + thisRead < lastData.Length)
                {
                    Array.Copy(buffer, 0, lastData, lastDataSzie, thisRead);
                    lastDataSzie = lastDataSzie + thisRead;
                }
                else
                {
                    var source = new ArraySegmentList<byte>(new List<ArraySegment<byte>>
                            {
                                new ArraySegment<byte>(lastData, 0, lastDataSzie),
                                new ArraySegment<byte>(buffer, 0, thisRead)
                            });

                    if (source.EndsWith(endMark))
                        return true;

                    for (int i = 0; i < endMark.Length; i++)
                    {
                        lastData[i] = source[source.Count - endMark.Length + i];
                    }

                    lastDataSzie = endMark.Length;
                }
            }

            return false;
        }
    }

    public class SocketSessionClosedEventArgs : EventArgs
    {
        public string IdentityKey { get; set; }
    }
}

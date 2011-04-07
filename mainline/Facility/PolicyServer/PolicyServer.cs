using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace SuperSocket.Facility.PolicyServer
{
    public abstract class PolicyServer : IGenericServer, IPolicyServer
    {
        protected ILogger Logger { get; private set; }

        private EndPoint m_LocalEndPoint;
        private string m_PolicyFile;
        private string m_PolicyRequest;
        private byte[] m_policyResponse;
        private bool m_Stopped = false;

        private Socket m_ListenSocket;

        private AutoResetEvent m_TcpClientConnected;

        private int m_ExpectedReceivedLength;

        private BufferManager m_BufferManager;

        private ConcurrentStack<SocketAsyncEventArgs> m_ReadPool;

        public bool Initialize(IGenericServerConfig config, ILogger logger)
        {
            Logger = logger;

            m_PolicyFile = config.Options.GetValue("policyFile");

            if (string.IsNullOrEmpty(m_PolicyFile))
            {
                logger.LogError("Configuration option policyFile is required!");
                return false;
            }

            if (!File.Exists(m_PolicyFile))
            {
                logger.LogError("The specified policyFile doesn't exist! " + m_PolicyFile);
                return false;
            }

            m_policyResponse = Encoding.UTF8.GetBytes(File.ReadAllText(m_PolicyFile, Encoding.UTF8));

            var policyRequest = config.Options.GetValue("policyRequest");
            if (!string.IsNullOrEmpty(policyRequest))
                m_PolicyRequest = policyRequest;

            m_ExpectedReceivedLength = Encoding.UTF8.GetByteCount(m_PolicyRequest);

            int port;

            if (!int.TryParse(config.Options.GetValue("port"), out port))
            {
                logger.LogError("Invalid port configuration!");
                return false;
            }

            var ip = config.Options.GetValue("ip");

            try
            {
                if (string.IsNullOrEmpty(ip) || "Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                    m_LocalEndPoint = new IPEndPoint(IPAddress.Any, port);
                else if ("IPv6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                    m_LocalEndPoint = new IPEndPoint(IPAddress.IPv6Any, port);
                else
                    m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Logger.LogError("Invalid ip configuration!", e);
                return false;
            }

            int concurrentCount = 100;

            m_BufferManager = new BufferManager(concurrentCount * m_ExpectedReceivedLength, m_ExpectedReceivedLength);

            var socketEventArgsList = new List<SocketAsyncEventArgs>(concurrentCount);

            for (var i = 0; i < concurrentCount; i++)
            {
                var e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
                m_BufferManager.SetBuffer(e);
                socketEventArgsList.Add(e);
            }

            m_ReadPool = new ConcurrentStack<SocketAsyncEventArgs>(socketEventArgsList);

            m_TcpClientConnected = new AutoResetEvent(false);

            return true;
        }

        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        void ProcessReceive(SocketAsyncEventArgs e)
        {
            PolicySession session = e.UserToken as PolicySession;
            if (session != null)
                session.ProcessReceive(e);
        }

        public void Start()
        {
            Async.Run(() => StartListen(), TaskCreationOptions.LongRunning);
        }

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.m_LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(this.m_LocalEndPoint);
                m_ListenSocket.Listen(100);

                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return;
            }

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

            while (!m_Stopped)
            {
                acceptEventArg.AcceptSocket = null;

                bool willRaiseEvent = true;

                try
                {
                    willRaiseEvent = m_ListenSocket.AcceptAsync(acceptEventArg);
                }
                catch (ObjectDisposedException)//listener has been stopped
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to accept new tcp client in async server!", e);
                    break;
                }

                if (!willRaiseEvent)
                    ProcessAccept(acceptEventArg);

                m_TcpClientConnected.WaitOne();
            }
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            var session = new PolicySession();
            session.Initialize(this, e.AcceptSocket, m_ExpectedReceivedLength);

            SocketAsyncEventArgs eventArgs;
            if (m_ReadPool.TryPop(out eventArgs))
            {
                eventArgs.UserToken = session;
                session.StartReceive(eventArgs);
            }
            else
            {
                Logger.LogError("No enough SocketAsyncEventArgs to process so many clients!");
            }
            
            m_TcpClientConnected.Set();
        }

        public void Stop()
        {
            if (m_Stopped)
                return;

            m_Stopped = true;

            if (m_ListenSocket != null)
            {
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }
        }

        public void ValidateSession(IPolicySession session, SocketAsyncEventArgs e, byte[] data)
        {
            e.UserToken = null;
            m_ReadPool.Push(e);
            Async.Run(() =>
                {
                    session.SendResponse(m_policyResponse);
                    session.Close();
                });
        }
    }
}

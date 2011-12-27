using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using SuperSocket.Common;

namespace PerformanceTestAgent.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Socket[] m_Sockets;
        private ConcurrentQueue<Socket> m_SocketQueue;
        private ConcurrentStack<SocketAsyncEventArgs> m_SocketReceivePool;
        private ConcurrentStack<SocketAsyncEventArgs> m_SocketSendPool;
        private BufferManager m_SendingBufferManager;
        private BufferManager m_ReceivingBufferManager;
        private int m_SendingBufferSize = 512;
        private int m_ReceivingBufferSize = 512;

        public MainViewModel()
        {
            m_StartCommand = new RelayCommand(ExecuteStart, CanExecuteStart);
            m_StopCommand = new RelayCommand(ExecuteStop, CanExecuteStop);
            ThreadPool.SetMaxThreads(1000, 1000);
        }

        private string m_TargetServer;

        public string TargetServer
        {
            get { return m_TargetServer; }
            set
            {
                m_TargetServer = value;
                RaisePropertyChanged("TargetServer");
            }
        }

        private int? m_Port;

        public int? Port
        {
            get { return m_Port; }
            set
            {
                m_Port = value;
                RaisePropertyChanged("Port");
            }
        }

        private int? m_ConnectionCount;

        public int? ConnectionCount
        {
            get { return m_ConnectionCount; }
            set
            {
                m_ConnectionCount = value;
                RaisePropertyChanged("ConnectionCount");
            }
        }

        private bool m_IsWaitResponse = false;

        public bool IsWaitResponse
        {
            get { return m_IsWaitResponse; }
            set
            {
                m_IsWaitResponse = value;
                RaisePropertyChanged("IsWaitResponse");
            }
        }

        private bool m_IsRunning = false;

        public bool IsRunning
        {
            get { return m_IsRunning; }
            set
            {
                m_IsRunning = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        private bool m_IsStopped;

        public bool IsStopped
        {
            get { return m_IsStopped; }
            set
            {
                m_IsStopped = value;
                RaisePropertyChanged("IsStopped");
            }
        }

        private ICommand m_StartCommand;

        public ICommand StartCommand
        {
            get { return m_StartCommand; }
        }

        private ICommand m_StopCommand;

        public ICommand StopCommand
        {
            get { return m_StopCommand; }
        }

        private void ExecuteStart()
        {
            m_Sockets = new Socket[m_ConnectionCount.Value];
            m_SocketQueue = new ConcurrentQueue<Socket>();
            m_ReceivingBufferManager = new BufferManager(m_ReceivingBufferSize * m_ConnectionCount.Value, m_ReceivingBufferSize);
            m_ReceivingBufferManager.InitBuffer();

            var socketReceiveEventList = new List<SocketAsyncEventArgs>(m_ConnectionCount.Value);

            for (int i = 0; i < m_ConnectionCount.Value; i++)
            {
                var socketEventArgs = new SocketAsyncEventArgs();
                socketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketReceiveEventArgs_Completed);
                m_ReceivingBufferManager.SetBuffer(socketEventArgs);
                socketReceiveEventList.Add(socketEventArgs);
            }

            m_SocketReceivePool = new ConcurrentStack<SocketAsyncEventArgs>(socketReceiveEventList);

            m_SendingBufferManager = new BufferManager(m_SendingBufferSize * m_ConnectionCount.Value, m_SendingBufferSize);
            m_SendingBufferManager.InitBuffer();

            var socketSendEventList = new List<SocketAsyncEventArgs>(m_ConnectionCount.Value);

            for (int i = 0; i < m_ConnectionCount.Value; i++)
            {
                var socketEventArgs = new SocketAsyncEventArgs();
                socketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketSendEventArgs_Completed);
                m_SendingBufferManager.SetBuffer(socketEventArgs);
                socketSendEventList.Add(socketEventArgs);
            }

            m_SocketSendPool = new ConcurrentStack<SocketAsyncEventArgs>(socketSendEventList);

            var ipAddress = IPAddress.Parse(m_TargetServer);
            var endPoint = new IPEndPoint(ipAddress, m_Port.Value);

            Task.Factory.StartNew(StartConnect, endPoint, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessSendWorker, TaskCreationOptions.LongRunning);

            IsRunning = true;
        }

        void StartConnect(object state)
        {
            var endPoint = (IPEndPoint)state;

            for (int i = 0; i < m_Sockets.Length; i++)
            {
                var connectSocketEventArgs = new SocketAsyncEventArgs();
                connectSocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(connectSocketEventArgs_Completed);
                connectSocketEventArgs.RemoteEndPoint = endPoint;
                connectSocketEventArgs.UserToken = i;

                if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, connectSocketEventArgs))
                    ProcessConnect(connectSocketEventArgs);
            }
        }

        void connectSocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var socketIndex = (int)e.UserToken;
                m_Sockets[socketIndex] = e.ConnectSocket;
                m_SocketQueue.Enqueue(e.ConnectSocket);
                string connectedMessage = string.Format("Socket {0} is connected!" + Environment.NewLine, socketIndex);
                OnContentAppend(connectedMessage);
            }
            else
            {
                OnContentAppend(e.SocketError + Environment.NewLine);
            }
        }

        private void socketReceiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void socketSendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                e.UserToken = null;
                m_SocketSendPool.Push(e);
                OnContentAppend("The connection was dropped!" + Environment.NewLine);
                return;
            }

            var sendingState = e.UserToken as SendingState;

            if (!sendingState.IsCompleted)
            {
                int encodedLen = sendingState.Encode(e.Buffer, e.Offset, m_SendingBufferSize);
                e.SetBuffer(e.Offset, encodedLen);

                if (!sendingState.Socket.SendAsync(e))
                {
                    ProcessSend(e);
                    return;
                }
            }

            e.UserToken = null;

            m_SocketSendPool.Push(e);

            if (!m_IsWaitResponse)
            {
                m_SocketQueue.Enqueue(sendingState.Socket);
                sendingState.Clear();
                sendingState = null;
                return;
            }

            SocketAsyncEventArgs receivingEventArgs;
            if (!m_SocketReceivePool.TryPop(out receivingEventArgs))
            {
                if (m_IsStopped)
                    return;

                Thread.Sleep(200);
            }

            if (m_IsStopped)
                return;

            receivingEventArgs.UserToken = new ReceivingState
            {
                RequireLength = sendingState.ExpectedReceiveLength,
                Socket = sendingState.Socket
            };


            if (!sendingState.Socket.ReceiveAsync(receivingEventArgs))
                ProcessReceive(receivingEventArgs);

            sendingState.Clear();
            sendingState = null;
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                m_SocketReceivePool.Push(e);
                OnContentAppend("The connection was dropped!" + Environment.NewLine);
                return;
            }

            var token = e.UserToken as ReceivingState;
            token.ReceivedLength += e.BytesTransferred;
            if (token.ReceivedLength >= token.RequireLength)
            {
                m_SocketReceivePool.Push(e);
                if(!m_IsStopped)
                    m_SocketQueue.Enqueue(token.Socket);
                OnContentAppend("Client received server's response!" + Environment.NewLine);
            }
            else
            {
                if (!m_IsStopped)
                {
                    if (!token.Socket.ReceiveAsync(e))
                        ProcessReceive(e);
                }
            }
        }

        private void ProcessSendWorker()
        {
            int sendTimes = 0;

            while (!m_IsStopped)
            {
                Socket socket;

                if (m_SocketQueue.TryDequeue(out socket))
                {
                    Task.Factory.StartNew(() => StartSend(socket));
                    sendTimes++;

                    if (sendTimes % 100 == 0)
                    {
                        Thread.Sleep(1);
                        sendTimes = 0;
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            IsRunning = false;
        }

        private void StartSend(Socket socket)
        {
            string message = Guid.NewGuid() + Environment.NewLine;

            SocketAsyncEventArgs sendingArgs;

            while (!m_SocketSendPool.TryPop(out sendingArgs))
            {
                if (m_IsStopped)
                    return;

                Thread.Sleep(200);
            }

            if (m_IsStopped)
                return;

            var state = new SendingState(socket, "ECHO " + message, Encoding.UTF8, Encoding.UTF8.GetByteCount(message));
            var encodedLen = state.Encode(sendingArgs.Buffer, sendingArgs.Offset, m_SendingBufferSize);
            sendingArgs.SetBuffer(sendingArgs.Offset, encodedLen);
            sendingArgs.UserToken = state;

            if (!socket.SendAsync(sendingArgs))
                ProcessSend(sendingArgs);
        }

        private bool CanExecuteStart()
        {
            if (string.IsNullOrEmpty(m_TargetServer))
                return false;

            if (!m_Port.HasValue)
                return false;

            if (!m_ConnectionCount.HasValue)
                return false;

            return true;
        }

        private bool CanExecuteStop()
        {
            return m_IsRunning;
        }

        private void ExecuteStop()
        {
            IsStopped = true;

            for (var i = 0; i < m_Sockets.Length; i++)
            {
                var socket = m_Sockets[i];

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch
                {
                }
            }

            m_SocketQueue = null;
        }

        private EventHandler<ContentAppendEventArgs> m_ContentAppend;

        public event EventHandler<ContentAppendEventArgs> ContentAppend
        {
            add { m_ContentAppend += value; }
            remove { m_ContentAppend -= value; }
        }

        private void OnContentAppend(string content)
        {
            var handler = m_ContentAppend;
            if (handler == null)
                return;

            handler(this, new ContentAppendEventArgs(content));
        }
    }
}

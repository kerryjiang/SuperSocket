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
        private BufferManager m_BufferManager;

        public MainViewModel()
        {
            m_StartCommand = new RelayCommand(ExecuteStart, CanExecuteStart);
            m_StopCommand = new RelayCommand(ExecuteStop, CanExecuteStop);
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
            int bufferSize = 1024;
            m_BufferManager = new BufferManager(bufferSize * m_ConnectionCount.Value, bufferSize);
            m_BufferManager.InitBuffer();

            var socketEventList = new List<SocketAsyncEventArgs>(m_ConnectionCount.Value);

            for (int i = 0; i < m_ConnectionCount.Value; i++)
            {
                var socketEventArgs = new SocketAsyncEventArgs();
                socketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketEventArgs_Completed);
                m_BufferManager.SetBuffer(socketEventArgs);
                socketEventList.Add(socketEventArgs);
            }

            m_SocketReceivePool = new ConcurrentStack<SocketAsyncEventArgs>(socketEventList);

            var ipAddress = IPAddress.Parse(m_TargetServer);
            var endPoint = new IPEndPoint(ipAddress, m_Port.Value);

            IsRunning = true;

            for (int i = 0; i < m_Sockets.Length; i++)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Sockets[i] = socket;
                string connectedMessage = string.Format("Socket {0} is connected!" + Environment.NewLine, i);
                Task.Factory.StartNew(() =>
                    {
                        socket.Connect(endPoint);
                        m_SocketQueue.Enqueue(socket);
                        OnContentAppend(connectedMessage);
                    }).ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            var innerException = t.Exception.InnerException;
                            if (innerException != null)
                                OnContentAppend(innerException.Message + Environment.NewLine + innerException.StackTrace + Environment.NewLine);
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }

            Task.Factory.StartNew(() => ProcessSendWorker(), TaskCreationOptions.LongRunning);
        }

        private void socketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
                return;

            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                m_SocketReceivePool.Push(e);
                OnContentAppend("The connection was dropped!" + Environment.NewLine);
                return;
            }

            var token = e.UserToken as AsyncUserToken;
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
            while (!m_IsStopped)
            {
                Socket socket;

                if (m_SocketQueue.TryDequeue(out socket))
                {
                    string message = Guid.NewGuid() + Environment.NewLine;
                    string line = "ECHO " + message;
                    socket.Send(Encoding.ASCII.GetBytes(line));
                    SocketAsyncEventArgs e;
                    if (m_SocketReceivePool.TryPop(out e))
                    {
                        e.UserToken = new AsyncUserToken
                            {
                                RequireLength = Encoding.ASCII.GetByteCount(message),
                                Socket = socket
                            };

                        if (!socket.ReceiveAsync(e))
                            ProcessReceive(e);
                    }
                }

                Thread.Sleep(1);
            }

            IsRunning = false;
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

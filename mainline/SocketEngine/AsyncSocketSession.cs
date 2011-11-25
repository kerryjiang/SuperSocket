using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    interface IAsyncSocketSession
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }
        void ProcessReceive(SocketAsyncEventArgs e);
        ILogger Logger { get; }
    }

    class AsyncSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>, IAsyncSocketSession
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {

        private int m_ConsumedDataSizeInCommand = 0;
        private int m_CurrentParsedLeft = 0;
        
        private AsyncSocketSender m_AsyncSender;

        public AsyncSocketSession(Socket client, ICommandReader<TCommandInfo> initialCommandReader)
            : base(client, initialCommandReader)
        {
            m_AsyncSender = new AsyncSocketSender(client);
        }

        ILogger IAsyncSocketSession.Logger
        {
            get { return AppServer.Logger; }
        }

        public override void Start()
        {
            SocketAsyncProxy.Initialize(Client, this);
            StartReceive(SocketAsyncProxy.SocketEventArgs);
            StartSession();
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (IsClosed)
                return;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public override void SendResponse(string message)
        {
            if (IsClosed)
                return;

            byte[] data = AppSession.Charset.GetBytes(message);

            if (IsClosed)
                return;

            SendResponse(data);
        }

        public override void SendResponse(byte[] data)
        {
            if(data == null || data.Length <= 0)
                return;

            if (IsClosed)
                return;

            try
            {
                m_AsyncSender.Send(data, 0, data.Length);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            try
            {
                m_AsyncSender.Send(data, offset, length);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            if (e.BytesTransferred <= 0)
            {
                Close(CloseReason.ClientClosing);
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Close(CloseReason.SocketError);
                return;
            }

            int bytesTransferred = e.BytesTransferred;
            int offset = e.Offset;

            while (bytesTransferred > 0)
            {
                int left;

                TCommandInfo commandInfo = FindCommand(e.Buffer, offset, bytesTransferred, true, out left);

                m_CurrentParsedLeft = left;

                if (IsClosed)
                    return;

                if (commandInfo == null)
                    break;

                try
                {
                    ExecuteCommand(commandInfo);
                }
                catch (Exception exc)
                {
                    AppServer.Logger.LogError(this, exc);
                    HandleExceptionalError(exc);
                }

                if (left > 0 && m_ConsumedDataSizeInCommand > 0)
                    left = Math.Max(left - m_ConsumedDataSizeInCommand, 0);

                m_ConsumedDataSizeInCommand = 0;
                    
                if (left <= 0)
                    break;

                bytesTransferred = left;
                offset = e.Offset + e.BytesTransferred - left;
            }

            //read the next block of data sent from the client
            StartReceive(e);
        }      

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }

        /// <summary>
        /// Receives the data.
        /// Process data synchronously, because command execution is waiting the received data
        /// </summary>
        /// <param name="storeSteram">The store steram.</param>
        /// <param name="length">The length.</param>
        public override void ReceiveData(Stream storeSteram, int length)
        {
            var e = this.SocketAsyncProxy.SocketEventArgs;

            byte[] buffer = e.Buffer;

            int thisRead = 0;
            int leftRead = length;
            int shouldRead = 0;

            if (m_CurrentParsedLeft > 0)
            {
                int pos = e.Offset + e.BytesTransferred - m_CurrentParsedLeft;
                int writeLen = Math.Min(m_CurrentParsedLeft, length);

                storeSteram.Write(e.Buffer, pos, writeLen);
                leftRead -= writeLen;
                m_ConsumedDataSizeInCommand = writeLen;
            }

            while (leftRead > 0)
            {
                shouldRead = Math.Min(buffer.Length, leftRead);
                thisRead = Client.Receive(buffer, 0, shouldRead, SocketFlags.None);
                AppSession.LastActiveTime = DateTime.Now;

                if (thisRead <= 0)
                {
                    //Slow speed? Wait a moment
                    Thread.Sleep(100);
                    continue;
                }

                storeSteram.Write(buffer, 0, thisRead);
                leftRead -= thisRead;
            }
        } 

        /// <summary>
        /// Receives the data.
        /// Process data synchronously, because command execution is waiting the received data
        /// </summary>
        /// <param name="storeSteram">The store steram.</param>
        /// <param name="endMark">The end mark.</param>
        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            var e = this.SocketAsyncProxy.SocketEventArgs;
            byte[] buffer = e.Buffer;
            byte[] lastData = new byte[endMark.Length];
            int lastDataSzie = 0;
            
            if (m_CurrentParsedLeft > 0)
            {
                int pos = e.Offset + e.BytesTransferred - m_CurrentParsedLeft;
                int writeLen;

                var result = buffer.SearchMark(pos, m_CurrentParsedLeft, endMark);
                var matched = false;

                if (result.HasValue && result.Value >= 0)
                {
                    writeLen = result.Value - pos + endMark.Length;
                    matched = true;
                }
                else//result.Value < 0
                {
                    writeLen = m_CurrentParsedLeft;
                }

                storeSteram.Write(e.Buffer, pos, writeLen);
                m_ConsumedDataSizeInCommand = writeLen;

                if (matched)
                    return;

                if (writeLen >= endMark.Length)
                {
                    Array.Copy(e.Buffer, pos + writeLen - endMark.Length, lastData, 0, endMark.Length);
                    lastDataSzie = endMark.Length;
                }
                else
                {
                    Array.Copy(e.Buffer, pos, lastData, 0, writeLen);
                    lastDataSzie = writeLen;
                }
            }

            int thisRead = 0;

            while (true)
            {
                thisRead = Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                AppSession.LastActiveTime = DateTime.Now;

                if (thisRead <= 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                storeSteram.Write(buffer, 0, thisRead);

                if(DetectEndMark(buffer, thisRead, endMark, lastData, ref lastDataSzie))
                    return;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.AsyncSocket;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.SocketServiceCore
{
    class AsyncSocketSession<T> : SocketSession<T>, IAsyncSocketSession
        where T : IAppSession, new()
    {
        IAsyncCommandReader m_CommandReader = new NormalAsyncCommandReader();

        protected override void Start(SocketContext context)
        {
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            SocketAsyncProxy.Initialize(Client, this, context);
            SayWelcome();
            StartReceive(SocketAsyncProxy.SocketEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (IsClosed)
                return;

            var socketContext = ((AsyncUserToken)e.UserToken).SocketContext;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception)
            {
                Close();
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public override void SendResponse(SocketContext context, string message)
        {
            if (IsClosed)
                return;

            if (string.IsNullOrEmpty(message) || !message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            byte[] data = context.Charset.GetBytes(message);

            if (IsClosed)
                return;

            try
            {
                Client.Send(data);
            }
            catch (Exception)
            {
                Close();
            }
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            if(data == null || data.Length <= 0)
                return;

            if (IsClosed)
                return;

            try
            {
                Client.Send(data);
            }
            catch (Exception)
            {
                Close();
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                Close();
                return;
            }

            byte[] commandData;

            SearhMarkResult result = m_CommandReader.FindCommand(e, token.SocketContext.NewLineData, out commandData);

            if (result.Status == SearhMarkStatus.Found)
            {
                //Initialize next command reader
                m_CommandReader = new NormalAsyncCommandReader(m_CommandReader);

                string commandLine = token.SocketContext.Charset.GetString(commandData);

                if (!string.IsNullOrEmpty(commandLine))
                    commandLine = commandLine.Trim();

                try
                {
                    ExecuteCommand(commandLine);
                }
                catch (Exception exc)
                {
                    LogUtil.LogError(AppServer, "An error occurred in session: " + this.SessionID, exc);
                    HandleExceptionalError(exc);
                }
                //read the next block of data send from the client
                StartReceive(e);
            }
            else
            {
                if (result.Status == SearhMarkStatus.FoundStart)
                    m_CommandReader = new PartMatchedAsyncCommandReader(m_CommandReader, result.Value);
                else
                    m_CommandReader = new NormalAsyncCommandReader(m_CommandReader);

                StartReceive(e);
                return;
            }             
        }      

        public override void ApplySecureProtocol(SocketContext context)
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
            byte[] buffer = this.SocketAsyncProxy.SocketEventArgs.Buffer;

            int thisRead = 0;
            int leftRead = length;
            int shouldRead = 0;

            AsyncUserToken token = this.SocketAsyncProxy.SocketEventArgs.UserToken as AsyncUserToken;

            var leftBuffer = m_CommandReader.GetLeftBuffer();

            if (leftBuffer != null && leftBuffer.Count > 0)
            {
                storeSteram.Write(leftBuffer.ToArray(), 0, leftBuffer.Count);
                leftRead -= leftBuffer.Count;
            }

            while (leftRead > 0)
            {
                shouldRead = Math.Min(buffer.Length, leftRead);
                thisRead = Client.Receive(buffer, 0, shouldRead, SocketFlags.None);

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
            byte[] buffer = this.SocketAsyncProxy.SocketEventArgs.Buffer;
            byte[] lastData = new byte[endMark.Length];
            int lastDataSzie = 0;

            int thisRead = 0;

            //var commandBuffer = m_CommandReader.GetLeftBuffer();

            //if (commandBuffer != null && commandBuffer.Count > 0)
            //{
            //    var result = commandBuffer.SearchMark(endMark);

            //    if (!result.HasValue)
            //    {
            //        byte[] commandBufferData = commandBuffer.ToArray();
            //        storeSteram.Write(commandBufferData, 0, commandBufferData.Length);
            //        commandBuffer.Clear();
            //    }
            //}

            while (true)
            {
                thisRead = Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

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

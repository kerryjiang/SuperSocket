using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.AsyncSocket;
using System.Collections;

namespace SuperSocket.SocketServiceCore
{
    public class AsyncSocketSession<T> : SocketSession<T>, IAsyncSocketSession
        where T : IAppSession, new()
    {
        AutoResetEvent m_SendReceiveResetEvent = new AutoResetEvent(true);

        protected override void Start(SocketContext context)
        {
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            SocketAsyncProxy.Initialize(Client, this, context);
            SayWelcome();
            StartReceive(SocketAsyncProxy.SocketEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            var socketContext = ((AsyncUserToken)e.UserToken).SocketContext;

            m_SendReceiveResetEvent.WaitOne();

            if (IsClosed)
                return;

            bool willRaiseEvent = Client.ReceiveAsync(e);
            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public override void SendResponse(SocketContext context, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            m_SendReceiveResetEvent.WaitOne();

            if (IsClosed)
                return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            var eventArgs = this.SocketAsyncProxy.SocketEventArgs;

            AsyncUserToken token = eventArgs.UserToken as AsyncUserToken;
            token.SendBuffer = context.Charset.GetBytes(message);
            token.Offset = 0;

            PrepareSendBuffer(eventArgs, token);

            if (!Client.SendAsync(eventArgs))
                ProcessSend(eventArgs);
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            if(data == null || data.Length <= 0)
                return;

            m_SendReceiveResetEvent.WaitOne();

            var eventArgs = this.SocketAsyncProxy.SocketEventArgs;

            AsyncUserToken token = eventArgs.UserToken as AsyncUserToken;
            token.SendBuffer = data;
            token.Offset = 0;

            PrepareSendBuffer(eventArgs, token);

            if (!Client.SendAsync(eventArgs))
                ProcessSend(eventArgs);
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                bool foundEnd = false;
                int newLineLen = token.SocketContext.NewLineData.Length;
                string commandLine = string.Empty;

                if (token.CommandSearchResult == null
                    || token.CommandSearchResult.ResultType == SearhMarkResultType.None
                    || token.CommandSearchResult.ResultType == SearhMarkResultType.FoundEnd
                    || token.CommandSearchResult.ResultType == SearhMarkResultType.MatchEnd)
                {

                    SearhMarkResult result = SearchMark(e.Buffer, e.Offset, e.BytesTransferred, token.SocketContext.NewLineData);

                    if (result.ResultType == SearhMarkResultType.FoundEnd || result.ResultType == SearhMarkResultType.MatchEnd)
                    {
                        foundEnd = true;

                        if (token.ReceiveBuffer != null && token.ReceiveBuffer.Count > 0)
                        {
                            token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(result.EndPos - e.Offset + 1));
                            commandLine = token.SocketContext.Charset.GetString(token.ReceiveBuffer.ToArray(), 0, token.ReceiveBuffer.Count - newLineLen);
                        }
                        else
                        {
                            commandLine = token.SocketContext.Charset.GetString(e.Buffer, e.Offset, result.EndPos - e.Offset + 1 - newLineLen);
                        }

                        if (result.ResultType != SearhMarkResultType.MatchEnd)
                        {
                            if (token.ReceiveBuffer == null)
                            {
                                token.ReceiveBuffer = new List<byte>();
                            }
                            else if (token.ReceiveBuffer.Count > 0)
                            {
                                token.ReceiveBuffer.Clear();
                            }

                            token.ReceiveBuffer.AddRange(e.Buffer.Skip(result.EndPos + 1).Take(e.BytesTransferred - result.EndPos - 1 - e.Offset));
                        }
                        else
                        {
                            if (token.ReceiveBuffer != null)
                            {
                                token.ReceiveBuffer.Clear();
                                token.ReceiveBuffer = null;
                            }
                        }
                    }
                    else if (result.ResultType == SearhMarkResultType.None)
                    {
                        if (token.ReceiveBuffer == null)
                        {
                            token.ReceiveBuffer = new List<byte>();
                        }

                        token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));
                    }
                    else if(result.ResultType == SearhMarkResultType.FoundStart)
                    {
                        if (token.ReceiveBuffer == null)
                        {
                            token.ReceiveBuffer = new List<byte>();
                        }

                        token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));
                    }

                    token.CommandSearchResult = result;
                }
                else if (token.CommandSearchResult.ResultType == SearhMarkResultType.FoundStart)
                {
                    var result = SearchMark(e.Buffer, e.Offset, token.CommandSearchResult.LeftMark.Length, token.CommandSearchResult.LeftMark);

                    if (result.ResultType == SearhMarkResultType.FoundEnd || result.ResultType == SearhMarkResultType.MatchEnd)
                    {
                        foundEnd = true;

                        token.ReceiveBuffer.AddRange(token.CommandSearchResult.LeftMark);
                        commandLine = token.SocketContext.Charset.GetString(token.ReceiveBuffer.ToArray(), 0, token.ReceiveBuffer.Count - newLineLen);
                        token.ReceiveBuffer.Clear();

                        if (e.BytesTransferred > token.CommandSearchResult.LeftMark.Length)
                        {
                            token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset + token.CommandSearchResult.LeftMark.Length).Take(e.BytesTransferred - token.CommandSearchResult.LeftMark.Length));
                        }
                        else
                        {
                            token.ReceiveBuffer = null;
                        }
                    }
                    else
                    {
                        token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));
                    }

                    token.CommandSearchResult = result;
                }

                if (foundEnd)
                {
                    commandLine = commandLine.Trim();

                    //this round receive has been finished, the buffer can be used for sending
                    m_SendReceiveResetEvent.Set();                    

                    try
                    {
                        ExecuteCommand(commandLine);
                    }
                    catch (Exception exc)
                    {
                        LogUtil.LogError(AppServer, exc);
                        HandleExceptionalError(exc);
                    }
                    //read the next block of data send from the client
                    StartReceive(e);
                }
                else
                {
                    //Continue receive
                    m_SendReceiveResetEvent.Set();
                    StartReceive(e);
                    return;
                }                
            }
            else
            {
                Close();
            }
        }

        public void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;

                //continue send
                if (token.SendBuffer.Length != 0)
                {
                    PrepareSendBuffer(e, token);

                    if (!Client.SendAsync(e))
                        ProcessSend(e);

                    return;
                }
                else
                {
                    //Send was finished, next round of send can be started
                    m_SendReceiveResetEvent.Set();
                }        
            }
            else
            {
                Close();
            }
        }

        private void PrepareSendBuffer(SocketAsyncEventArgs e, AsyncUserToken token)
        {
            int leftBytes = token.SendBuffer.Length - token.Offset;

            if (e.Buffer.Length >= leftBytes)
            {
                Buffer.BlockCopy(token.SendBuffer, token.Offset, e.Buffer, 0, leftBytes);
                e.SetBuffer(0, leftBytes);
                token.SendBuffer = new byte[0];
                token.Offset = 0;
            }
            else
            {
                Buffer.BlockCopy(token.SendBuffer, token.Offset, e.Buffer, 0, e.Buffer.Length);
                e.SetBuffer(0, e.Buffer.Length);
                token.Offset = token.Offset + e.Buffer.Length;
            }            
        }

        public override void Close()
        {
            if (!IsClosed)
            {
                SocketAsyncProxy.Reset();
                base.Close();
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

            if (token.ReceiveBuffer != null && token.ReceiveBuffer.Count > 0)
            {
                storeSteram.Write(token.ReceiveBuffer.ToArray(), 0, token.ReceiveBuffer.Count);
                leftRead -= token.ReceiveBuffer.Count;
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

            while (true)
            {
                thisRead = Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                if (thisRead > 0)
                {
                    if (thisRead >= endMark.Length)
                    {
                        if (EndsWith(buffer, 0, thisRead, endMark))
                        {
                            storeSteram.Write(buffer, 0, thisRead);
                            return;
                        }
                        else
                        {
                            storeSteram.Write(buffer, 0, thisRead);
                            Array.Copy(buffer, thisRead - endMark.Length - 1, lastData, 0, endMark.Length);
                            lastDataSzie = endMark.Length;
                        }
                    }
                    else
                    {
                        bool matched = false;

                        int searchIndex = endMark.Length - 1;

                        for (int i = thisRead - 1; i >= 0 && searchIndex >= 0; i--, searchIndex--)
                        {
                            if (endMark[searchIndex] != buffer[i])
                            {
                                matched = false;
                                break;
                            }
                            else
                            {
                                matched = true;
                            }
                        }

                        if (lastDataSzie > 0)
                        {
                            for (int i = lastDataSzie - 1; i >= 0 && searchIndex >= 0; i--, searchIndex--)
                            {
                                if (endMark[searchIndex] != lastData[i])
                                {
                                    matched = false;
                                    break;
                                }
                                else
                                {
                                    matched = true;
                                }
                            }
                        }

                        if (matched && searchIndex < 0)
                        {
                            storeSteram.Write(buffer, 0, thisRead);
                            return;
                        }
                        else
                        {
                            storeSteram.Write(buffer, 0, thisRead);

                            if (lastDataSzie + thisRead <= lastData.Length)
                            {
                                Array.Copy(buffer, 0, lastData, lastDataSzie, thisRead);
                                lastDataSzie = lastDataSzie + thisRead;
                            }
                            else
                            {
                                Array.Copy(lastData, thisRead + lastDataSzie - lastData.Length, lastData, 0, lastData.Length - thisRead);
                                Array.Copy(buffer, 0, lastData, lastDataSzie, thisRead);
                                lastDataSzie = endMark.Length;
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        private SearhMarkResult SearchMark(byte[] source, byte[] mark)
        {
            return SearchMark(source, 0, source.Length, mark);
        }

        private SearhMarkResult SearchMark(byte[] source, int offset, int length, byte[] mark)
        {
            int pos = offset;
            int matchLevel = -1;
            int endOffset = offset + length - 1;

            while (pos <= endOffset)
            {
                for (matchLevel = 0; matchLevel < mark.Length; matchLevel++)
                {
                    //reach the end
                    if (pos + matchLevel > endOffset)
                    {
                        break;
                    }

                    if (source[pos + matchLevel].CompareTo(mark[matchLevel]) != 0)
                    {
                        matchLevel = -1;
                        break;
                    }
                }

                if (matchLevel < 0)
                {
                    pos++;
                }
                else
                {
                    matchLevel--;
                    if (matchLevel == (mark.Length - 1))
                    {
                        if ((pos + matchLevel) == (offset + length - 1))
                        {
                            return new SearhMarkResult
                            {
                                ResultType = SearhMarkResultType.MatchEnd,
                                EndPos = pos + matchLevel
                            };
                        }
                        else
                        {
                            return new SearhMarkResult
                            {
                                ResultType = SearhMarkResultType.FoundEnd,
                                EndPos = pos + matchLevel
                            };
                        }
                    }
                    else
                    {
                        return new SearhMarkResult
                        {
                            ResultType = SearhMarkResultType.FoundStart,
                            EndPos = pos + matchLevel,
                            LeftMark = mark.Skip(matchLevel + 1).ToArray()
                        };
                    }
                }
            }

            return new SearhMarkResult
            {
                ResultType = SearhMarkResultType.None,
                EndPos = 0
            };
        }
    }


    class SearhMarkResult
    {
        public SearhMarkResultType ResultType { get; set; }
        public int EndPos { get; set; }
        public byte[] LeftMark { get; set; }
    }

    enum SearhMarkResultType
    {
        None,
        FoundStart,
        FoundEnd,
        MatchEnd
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using SuperSocket.ClientEngine.Protocol;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.WebSocket;
using WebSocket.Test;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class WebSocketRawTest : WebSocketTestBase
    {
        private Encoding m_Encoding;
        protected string NewLine { get; private set; }

        public WebSocketRawTest()
        {
            m_Encoding = new UTF8Encoding();
            NewLine = "\r\n";
        }

        public override void Setup()
        {
            var appServer = AppServer;

            appServer.NewSessionConnected += appServer_NewSessionConnected;
            appServer.SessionClosed += appServer_SessionClosed;
        }

        void appServer_SessionClosed(AppSession session, CloseReason value)
        {
            
        }

        void appServer_NewSessionConnected(AppSession session)
        {
            
        }

        protected virtual string SubProtocol
        {
            get { return string.Empty; }
        }

        protected void Handshake(out Socket socket, out Stream stream)
        {
            Handshake(SubProtocol, out socket, out stream);
        }

        protected virtual void Handshake(string protocol, out Socket socket, out Stream stream)
        {
            var ip = "127.0.0.1";
            var port = AppServer.Config.Port;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, m_Encoding, false);
            var writer = new StreamWriter(stream, m_Encoding, 1024 * 10);

            writer.Write("GET /websock HTTP/1.1");
            writer.Write(NewLine);
            writer.Write("Upgrade: WebSocket");
            writer.Write(NewLine);
            writer.Write("Connection: Upgrade");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Key2: 12998 5 Y3 1  .P00");
            writer.Write(NewLine);
            writer.Write("Host: example.com");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Key1: 4 @1  46546xW%0l 1 5");
            writer.Write(NewLine);
            writer.Write("Origin: http://example.com");
            writer.Write(NewLine);
            
            if (!string.IsNullOrEmpty(protocol))
            {
                writer.Write("Sec-WebSocket-Protocol: {0}", protocol);
                writer.Write(NewLine);
            }
            
            writer.Write(NewLine);
            
            string secKey = "^n:ds[4U";
            writer.Write(secKey);
            writer.Flush();

            //secKey.ToList().ForEach(c => Console.WriteLine((int)c));

            while (!string.IsNullOrEmpty(reader.ReadLine()))
                continue;

            char[] buffer = new char[16];

            int totalRead = 0;

            while (totalRead < 16)
            {
                int read = reader.Read(buffer, totalRead, buffer.Length - totalRead);

                if (read <= 0)
                    Assert.Fail("Connection closed!");

                totalRead += read;
            }

            Assert.AreEqual("8jKS'y:G*Co,Wxa-", new string(buffer));
        }

        [Test]
        public void HandshakeTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test]
        public virtual void MessageTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            ArraySegmentList receivedBuffer = new ArraySegmentList();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string currentCommand = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + currentCommand);

                stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
                byte[] data = m_Encoding.GetBytes(currentCommand);
                stream.Write(data, 0, data.Length);
                stream.Write(new byte[] { WebSocketConstant.EndByte }, 0, 1);
                stream.Flush();

                ReceiveMessage(stream, receivedBuffer, data.Length + 2);
                Assert.AreEqual(data.Length + 2, receivedBuffer.Count);
                Assert.AreEqual(WebSocketConstant.StartByte, receivedBuffer[0]);
                Assert.AreEqual(WebSocketConstant.EndByte, receivedBuffer[receivedBuffer.Count - 1]);
                Assert.AreEqual(currentCommand, m_Encoding.GetString(receivedBuffer.ToArrayData(1, receivedBuffer.Count - 2)));
                receivedBuffer.ClearSegements();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test]
        public virtual void MessageBatchTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(out socket, out stream);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            ArraySegmentList receivedBuffer = new ArraySegmentList();

            for (int i = 0; i < 10; i++)
            {
                var sentMessages = new string[10];
                var sentLengths = new int[sentMessages.Length];

                for (int j = 0; j < sentMessages.Length; j++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, Math.Min(messageSource.Length - 1, startPos + 1 + 5));

                    string currentCommand = messageSource.Substring(startPos, endPos - startPos);
                    sentMessages[j] = currentCommand;

                    Console.WriteLine("Client:" + currentCommand);

                    stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
                    byte[] data = m_Encoding.GetBytes(currentCommand);
                    sentLengths[j] = data.Length + 2;
                    stream.Write(data, 0, data.Length);
                    stream.Write(new byte[] { WebSocketConstant.EndByte }, 0, 1);
                }

                stream.Flush();

                for (var j = 0; j < sentMessages.Length; j++)
                {
                    Console.WriteLine("Expected: " + sentLengths[j]);
                    ReceiveMessage(stream, receivedBuffer, sentLengths[j]);
                    string message = m_Encoding.GetString(receivedBuffer.ToArrayData(1, receivedBuffer.Count - 2));
                    Console.WriteLine("E:" + sentMessages[j]);
                    Console.WriteLine("A:" + message);
                    Assert.AreEqual(WebSocketConstant.StartByte, receivedBuffer[0]);
                    Assert.AreEqual(WebSocketConstant.EndByte, receivedBuffer[receivedBuffer.Count - 1]);
                    Assert.AreEqual(sentMessages[j], message);
                    Assert.AreEqual(sentLengths[j], receivedBuffer.Count);
                    receivedBuffer.ClearSegements();
                    Console.WriteLine("Passed " + j);
                }
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        protected void ReceiveMessage(Stream stream, ArraySegmentList commandBuffer, int predictCount)
        {
            byte[] buffer = new byte[1024];
            int thisRead = 0;
            int left = predictCount;

            while ((thisRead = stream.Read(buffer, 0, Math.Min(left, buffer.Length))) > 0)
            {
                Console.WriteLine("Current read: {0}", thisRead);
                commandBuffer.AddSegment(buffer, 0, thisRead, true);
                left -= thisRead;

                if (left <= 0)
                    break;
            }
        }
    }
}

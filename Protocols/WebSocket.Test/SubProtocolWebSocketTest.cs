using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SuperSocket.ClientEngine.Protocol;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.WebSocket;

namespace SuperWebSocketTest
{
    [TestFixture]
    public class SubProtocolWebSocketTest : WebSocketRawTest
    {
        private Encoding m_Encoding;
        
        public SubProtocolWebSocketTest()
            : base()
        {
        
        }
        
        //[TestFixtureSetUp]
        //public override void Setup()
        //{
        //    m_Encoding = new UTF8Encoding();

        //    Setup(new WebSocketServer(new BasicSubProtocol()), c =>
        //    {
        //        c.Port = 2012;
        //        c.Ip = "Any";
        //        c.MaxConnectionNumber = 100;
        //        c.Mode = SocketMode.Tcp;
        //        c.Name = "SuperWebSocket Server";
        //    });
        //}

        protected override string SubProtocol
        {
            get
            {
                return "Basic";
            }
        }

        [Test]
        public void TestEcho()
        {
            Socket socket;
            Stream stream;

            Handshake(SubProtocol, out socket, out stream);

            StringBuilder sb = new StringBuilder();

            sb.Append("ECHO");

            var parameters = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                var p = Guid.NewGuid().ToString();
                sb.Append(" ");
                sb.Append(p);
                parameters.Add(p);
            }

            stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
            byte[] data = m_Encoding.GetBytes(sb.ToString());
            stream.Write(data, 0, data.Length);
            stream.Write(new byte[] { WebSocketConstant.EndByte }, 0, 1);
            stream.Flush();

            var receivedBuffer = new ArraySegmentList();

            foreach (var p in parameters)
            {
                ReceiveMessage(stream, receivedBuffer, Encoding.UTF8.GetBytes(p).Length + 2);
                string rp = m_Encoding.GetString(receivedBuffer.ToArrayData(1, receivedBuffer.Count - 2));
                Console.WriteLine(rp);
                Assert.AreEqual(p, rp);
                receivedBuffer.ClearSegements();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test, Timeout(5000)]
        public override void MessageTransferTest()
        {
            Socket socket;
            Stream stream;

            Handshake(SubProtocol, out socket, out stream);

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
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, Math.Min(messageSource.Length - 1, startPos + 1 + 100));

                string currentCommand = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + currentCommand);

                stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
                byte[] data = m_Encoding.GetBytes("ECHO " + currentCommand);
                stream.Write(data, 0, data.Length);
                stream.Write(new byte[] { WebSocketConstant.EndByte }, 0, 1);
                stream.Flush();

                int requredCount = m_Encoding.GetByteCount(currentCommand) + 2;
                Console.WriteLine("Require:" + requredCount);

                ReceiveMessage(stream, receivedBuffer, requredCount);
                Assert.AreEqual(requredCount, receivedBuffer.Count);
                Assert.AreEqual(WebSocketConstant.StartByte, receivedBuffer[0]);
                Assert.AreEqual(WebSocketConstant.EndByte, receivedBuffer[receivedBuffer.Count - 1]);
                Assert.AreEqual(currentCommand, m_Encoding.GetString(receivedBuffer.ToArrayData(1, receivedBuffer.Count - 2)));
                receivedBuffer.ClearSegements();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        [Test, Timeout(5000)]
        public override void MessageBatchTransferTest()
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

            byte[] commandNameData = Encoding.UTF8.GetBytes("ECHO ");

            for (int i = 0; i < 10; i++)
            {
                var sentMessages = new string[10];
                var sentLengths = new int[sentMessages.Length];

                for (int j = 0; j < sentMessages.Length; j++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                    string currentCommand = messageSource.Substring(startPos, endPos - startPos);
                    sentMessages[j] = currentCommand;

                    Console.WriteLine("Client:" + currentCommand);

                    stream.Write(new byte[] { WebSocketConstant.StartByte }, 0, 1);
                    byte[] data = m_Encoding.GetBytes(currentCommand);
                    sentLengths[j] = data.Length + 2;
                    stream.Write(commandNameData, 0, commandNameData.Length);
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.WebSocket;
using WebSocket4Net;

namespace WebSocket.Test
{
    [TestFixture]
    public class WebSocketClientTestHybi00 : WebSocketClientTest
    {
        public WebSocketClientTestHybi00()
            : base(WebSocketVersion.DraftHybi00)
        {

        }

        [Test]
        public override void SendDataTest()
        {
            
        }
    }

    [TestFixture]
    public class WebSocketClientTestHybi10 : WebSocketClientTest
    {
        public WebSocketClientTestHybi10()
            : base(WebSocketVersion.DraftHybi10)
        {

        }
    }

    [TestFixture]
    public class WebSocketClientTestRFC6455 : WebSocketClientTest
    {
        public WebSocketClientTestRFC6455()
            : base(WebSocketVersion.Rfc6455)
        {

        }

    //    [Test, Repeat(10)]
    //    public void SendBinaryMessageTest()
    //    {
    //        try
    //        {
    //            WebSocketServer.NewDataReceived -= new SessionHandler<WebSocketSession, byte[]>(WebSocketServer_NewDataReceived);

    //            var webSocketClient = CreateClient(Version);

    //            StringBuilder sb = new StringBuilder();

    //            for (int i = 0; i < 10; i++)
    //            {
    //                sb.Append(Guid.NewGuid().ToString());
    //            }

    //            string messageSource = sb.ToString();

    //            Random rd = new Random();

    //            for (int i = 0; i < 100; i++)
    //            {
    //                int startPos = rd.Next(0, messageSource.Length - 2);
    //                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

    //                string message = messageSource.Substring(startPos, endPos - startPos);
    //                var data = Encoding.UTF8.GetBytes("ECHO " + message);

    //                webSocketClient.Send(data, 0, data.Length);

    //                Console.WriteLine("Client:" + message);

    //                if (!MessageReceiveEvent.WaitOne(10000))
    //                    Assert.Fail("Cannot get response in time!");

    //                Assert.AreEqual(message, CurrentMessage);
    //            }

    //            webSocketClient.Close();

    //            if (!CloseEvent.WaitOne(1000))
    //                Assert.Fail("Failed to close session ontime");
    //        }
    //        catch (Exception e)
    //        {
    //            throw e;
    //        }
    //        finally
    //        {
    //            WebSocketServer.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(WebSocketServer_NewDataReceived);
    //        }
    //    }
    }

    public abstract class WebSocketClientTest : WebSocketTestBase
    {
        private readonly WebSocketVersion m_Version;

        protected WebSocketVersion Version
        {
            get { return m_Version; }
        }

        public WebSocketClientTest(WebSocketVersion version)
        {
            m_Version = version;
        }

        //[TestFixtureSetUp]
        //public override void Setup()
        //{
        //    Setup(new WebSocketServer(new BasicSubProtocol()), c =>
        //    {
        //        c.Port = 2012;
        //        c.Ip = "Any";
        //        c.MaxConnectionNumber = 100;
        //        c.MaxRequestLength = 100000;
        //        c.Name = "SuperWebSocket Server";
        //    });
        //}

        [Test]
        public void ConnectionTest()
        {
            var webSocketClient = CreateClient(m_Version);

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            webSocketClient.Close();

            if (!CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        [Test, Repeat(10)]
        public void SendMessageTest()
        {
            var webSocketClient = CreateClient(m_Version);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                webSocketClient.Send("ECHO " + message);

                Console.WriteLine("Client:" + message);

                if (!MessageReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, CurrentMessage);
            }

            webSocketClient.Close();

            if (!CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }
  
        [Test, Repeat(10)]
        public virtual void SendDataTest()
        {
            var webSocketClient = CreateClient(m_Version);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + message);
                var data = Encoding.UTF8.GetBytes(message);
                webSocketClient.Send(data, 0, data.Length);

                if (!this.DataReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, Encoding.UTF8.GetString(CurrentData));
            }

            webSocketClient.Close();

            if (!CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }
        
        [Test, Repeat(10)]
        public void CloseWebSocketTest()
        {
            var webSocketClient = CreateClient(m_Version);

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            webSocketClient.Send("QUIT");

            if (!CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        //[Test]
        //public virtual void CommandFilterTest()
        //{
        //    var webSocketClient = CreateClient(m_Version);

        //    var oldExecutingCount = CountSubCommandFilterAttribute.ExecutingCount;
        //    var oldExecutedCount = CountSubCommandFilterAttribute.ExecutedCount;
            
        //    for (int i = 0; i < 100; i++)
        //    {
        //        webSocketClient.Send("ECHO " + Guid.NewGuid().ToString());

        //        if (!this.MessageReceiveEvent.WaitOne(1000))
        //            Assert.Fail("Cannot get response in time!");

        //        Thread.Sleep(10);

        //        Assert.AreEqual(oldExecutingCount + i + 1, CountSubCommandFilterAttribute.ExecutingCount);
        //        Assert.AreEqual(oldExecutedCount + i + 1, CountSubCommandFilterAttribute.ExecutedCount);
        //    }

        //    webSocketClient.Close();

        //    if (!CloseEvent.WaitOne(1000))
        //        Assert.Fail("Failed to close session ontime");
        //}
    }
}

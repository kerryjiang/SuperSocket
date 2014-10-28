using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using WebSocket.Test;
using WebSocket4Net;

namespace WebSocket.Test
{
    public abstract class WebSocketTestBase : BootstrapTestBase
    {
        protected AppServer AppServer { get; private set; }

        protected AutoResetEvent MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent DataReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent CloseEvent = new AutoResetEvent(false);
        protected string CurrentMessage { get; private set; }
        protected byte[] CurrentData { get; private set; }

        public WebSocketTestBase()
        {

        }

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            StartBootstrap("Basic.Config");
            AppServer = BootStrap.AppServers.FirstOrDefault() as AppServer;
            AppServer.NewRequestReceived += AppServer_NewRequestReceived;
        }

        void AppServer_NewRequestReceived(AppSession session, StringPackageInfo requestInfo)
        {
            session.Send(requestInfo.Body);
        }

        protected void WebSocketServer_NewDataReceived(AppSession session, byte[] e)
        {
            session.Send(e, 0, e.Length);
        }

        [SetUp]
        public void StartServer()
        {
            AppServer.Start();
        }

        [TearDown]
        public void StopServer()
        {
            AppServer.Stop();
        }

        protected WebSocket4Net.WebSocket CreateClient()
        {
            return CreateClient(WebSocketVersion.Rfc6455, true);
        }

        protected WebSocket4Net.WebSocket CreateClient(WebSocketVersion version)
        {
            return CreateClient(version, true);
        }

        protected WebSocket4Net.WebSocket CreateClient(WebSocketVersion version, bool autoConnect)
        {
            var webSocketClient = new WebSocket4Net.WebSocket(string.Format("ws://127.0.0.1:{0}/websocket", AppServer.Config.Port), "basic", version);
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.DataReceived += new EventHandler<DataReceivedEventArgs>(webSocketClient_DataReceived);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);

            if (autoConnect)
            {
                webSocketClient.Open();

                if (!OpenedEvent.WaitOne(1000))
                    Assert.Fail("Failed to open");
            }
            
            return webSocketClient;
        }

        void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            CurrentMessage = e.Message;
            MessageReceiveEvent.Set();
        }

        void webSocketClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            CurrentData = e.Data;
            DataReceiveEvent.Set();
        }

        void webSocketClient_Closed(object sender, EventArgs e)
        {
            CloseEvent.Set();
        }

        void webSocketClient_Opened(object sender, EventArgs e)
        {
            OpenedEvent.Set();
        }
    }
}

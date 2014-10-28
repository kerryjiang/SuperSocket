using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;

namespace WebSocket.Test
{
    [TestFixture]
    public class WebSocketFramingTest : WebSocketTestBase
    {
        [Test]
        public void SendEmptyMessage()
        {
            VerifySendMessage("");
        }

        [Test]
        public void SendMessageSize125()
        {
            VerifySendMessage(125);
        }

        [Test]
        public void SendMessageSize126()
        {
            VerifySendMessage(126);
        }

        [Test]
        public void SendMessageSize127()
        {
            VerifySendMessage(127);
        }

        [Test]
        public void SendMessageSize128()
        {
            VerifySendMessage(128);
        }

        [Test]
        public void SendMessageSize65535()
        {
            VerifySendMessage(65535);
        }

        [Test]
        public void SendMessageSize65536()
        {
            VerifySendMessage(65536);
        }

        private void VerifySendMessage(int length)
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                sb.Append(Guid.NewGuid().ToString());

                if (sb.Length >= length)
                    break;
            }

            var message = sb.ToString().Substring(0, length);

            VerifySendMessage(message);
        }

        private void VerifySendMessage(string message)
        {
            var websocket = CreateClient();

            websocket.Send(message);

            if (!MessageReceiveEvent.WaitOne())
                Assert.Fail("Failed to receive message one time");

            Assert.AreEqual(message, CurrentMessage);

            websocket.Close();
        }

        [Test]
        public void SendEmptyData()
        {
            VerifySendData(new byte[0]);
        }

        [Test]
        public void SendDataSize125()
        {
            VerifySendData(125);
        }

        [Test]
        public void SendDataSize126()
        {
            VerifySendData(126);
        }

        [Test]
        public void SendDataSize127()
        {
            VerifySendData(127);
        }

        [Test]
        public void SendDataSize128()
        {
            VerifySendData(128);
        }

        [Test]
        public void SendDataSize65535()
        {
            VerifySendData(65535);
        }

        [Test]
        public void SendDataSize65536()
        {
            VerifySendData(65536);
        }

        private void VerifySendData(int length)
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                sb.Append(Guid.NewGuid().ToString());

                if (sb.Length >= length)
                    break;
            }

            var message = sb.ToString().Substring(0, length);

            VerifySendData(Encoding.ASCII.GetBytes(message));
        }

        private void VerifySendData(byte[] data)
        {
            var websocket = CreateClient();

            var rawData = new byte[data.Length];

            data.CopyTo(rawData, 0);

            websocket.Send(data, 0, data.Length);

            if (!DataReceiveEvent.WaitOne())
                Assert.Fail("Failed to receive data one time");

            Assert.AreEqual(rawData, CurrentData);

            websocket.Close();
        }
    }
}

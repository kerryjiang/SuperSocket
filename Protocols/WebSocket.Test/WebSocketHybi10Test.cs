using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SuperSocket.ClientEngine.Protocol;
using SuperSocket.Common;


namespace SuperWebSocketTest
{
    public class WebSocketHybi10Test : WebSocketRawTest
    {
        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        protected override void Handshake(string protocol, out Socket socket, out System.IO.Stream stream)
        {
            var ip = "127.0.0.1";
            var port = 2012;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, new UTF8Encoding(), false);
            var writer = new StreamWriter(stream, new UTF8Encoding(), 1024 * 10);

            var secKey = Guid.NewGuid().ToString().Substring(0, 5);

            writer.Write("GET /websock HTTP/1.1");
            writer.Write(NewLine);
            writer.Write("Upgrade: WebSocket");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Version: 8");
            writer.Write(NewLine);
            writer.Write("Connection: Upgrade");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Key: " + secKey);
            writer.Write(NewLine);
            writer.Write("Host: example.com");
            writer.Write(NewLine);
            writer.Write("Origin: http://example.com");
            writer.Write(NewLine);

            if (!string.IsNullOrEmpty(protocol))
            {
                writer.Write("Sec-WebSocket-Protocol: {0}", protocol);
                writer.Write(NewLine);
            }

            writer.Write(NewLine);
            writer.Flush();

            reader.ReadLine();

            var response = new StringDictionary();

            while (true)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                var arr = line.Split(':');

                response[arr[0]] = arr[1].Trim();
            }

            var expectedKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secKey + m_Magic)));

            Assert.AreEqual(expectedKey, response["Sec-WebSocket-Accept"]);
        }

        [Test]
        public void MultipleVersionTest()
        {
            var ip = "127.0.0.1";
            var port = 2012;

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var address = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(address);

            var stream = new NetworkStream(socket);

            var reader = new StreamReader(stream, new UTF8Encoding(), false);
            var writer = new StreamWriter(stream, new UTF8Encoding(), 1024 * 10);

            var secKey = Guid.NewGuid().ToString().Substring(0, 5);

            writer.Write("GET /websock HTTP/1.1");
            writer.Write(NewLine);
            writer.Write("Upgrade: WebSocket");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Version: 10");
            writer.Write(NewLine);
            writer.Write("Connection: Upgrade");
            writer.Write(NewLine);
            writer.Write("Sec-WebSocket-Key: " + secKey);
            writer.Write(NewLine);
            writer.Write("Host: example.com");
            writer.Write(NewLine);
            writer.Write("Origin: http://example.com");
            writer.Write(NewLine);

            writer.Write(NewLine);
            writer.Flush();

            reader.ReadLine();

            var response = new StringDictionary();

            while (true)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrEmpty(line))
                    break;

                var arr = line.Split(':');

                response[arr[0]] = arr[1].Trim();
            }

            var versions = response["Sec-WebSocket-Version"];

            Console.WriteLine("Sec-WebSocket-Version: {0}", versions);
            Assert.AreEqual("8, 13", versions);

            socket.Close();
        }

        private static Random m_Random = new Random();

        private void GenerateMask(byte[] mask, int offset)
        {
            for (var i = offset; i < offset + 4; i++)
            {
                mask[i] = (byte)m_Random.Next(0, 255);
            }
        }

        private void MaskData(byte[] rawData, int offset, int length, byte[] mask, int maskOffset)
        {
            for (var i = 0; i < length; i++)
            {
                var pos = offset + i;
                rawData[pos] = (byte)(rawData[pos] ^ mask[maskOffset + i % 4]);
            }
        }

        [Test]
        public override void MessageTransferTest()
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

                var data = Encoding.UTF8.GetBytes(currentCommand);

                Console.WriteLine("Client Length:" + data.Length);

                int dataLen = SendMessage(stream, 1, data);
                stream.Flush();

                ReceiveMessage(stream, receivedBuffer, dataLen);

                Assert.AreEqual(dataLen, receivedBuffer.Count);
                Assert.AreEqual(0x01, receivedBuffer[0] & 0x01);
                Assert.AreEqual(0x80, receivedBuffer[0] & 0x80);
                Assert.AreEqual(0x00, receivedBuffer[1] & 0x80);

                int skip = 2;

                if (data.Length < 126)
                    Assert.AreEqual(data.Length, (int)(receivedBuffer[1] & 0x7F));
                else if (data.Length < 65536)
                {
                    Assert.AreEqual(126, (int)(receivedBuffer[1] & 0x7F));
                    var sizeData = receivedBuffer.ToArrayData(2, 2);
                    Assert.AreEqual(data.Length, (int)sizeData[0] * 256 + (int)sizeData[1]);
                    skip += 2;
                }
                else
                {
                    Assert.AreEqual(127, (int)(receivedBuffer[1] & 0x7F));

                    var sizeData = receivedBuffer.ToArrayData(2, 8);

                    long len = 0;
                    int n = 1;

                    for (int k = 7; k >= 0; k--)
                    {
                        len += (int)sizeData[k] * n;
                        n *= 256;
                    }

                    Assert.AreEqual(data.Length, len);
                    skip += 8;
                }

                Assert.AreEqual(currentCommand, Encoding.UTF8.GetString(receivedBuffer.ToArrayData(skip, data.Length)));

                receivedBuffer.ClearSegements();
            }

            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
        }

        [Test]
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

            for (int i = 0; i < 10; i++)
            {
                var sentMessages = new string[10];
                var sentMessageSizes = new int[10];
                var sentLengths = new int[sentMessages.Length];

                for (int j = 0; j < sentMessages.Length; j++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, Math.Min(messageSource.Length - 1, startPos + 1 + 5));

                    string currentCommand = messageSource.Substring(startPos, endPos - startPos);
                    sentMessages[j] = currentCommand;

                    Console.WriteLine("Client:" + currentCommand);
                    byte[] data = Encoding.UTF8.GetBytes(currentCommand);
                    Console.WriteLine("Client Length:" + data.Length);
                    sentMessageSizes[j] = data.Length;
                    int dataLen = SendMessage(stream, 1, data);
                    sentLengths[j] = dataLen;
                }

                stream.Flush();

                for (var j = 0; j < sentMessages.Length; j++)
                {
                    Console.WriteLine("Expected: " + sentLengths[j]);
                    ReceiveMessage(stream, receivedBuffer, sentLengths[j]);

                    int mlen = sentMessageSizes[j];

                    Assert.AreEqual(sentLengths[j], receivedBuffer.Count);
                    Assert.AreEqual(0x01, receivedBuffer[0] & 0x01);
                    Assert.AreEqual(0x80, receivedBuffer[0] & 0x80);
                    Assert.AreEqual(0x00, receivedBuffer[1] & 0x80);

                    int skip = 2;

                    if (mlen < 126)
                        Assert.AreEqual(mlen, (int)(receivedBuffer[1] & 0x7F));
                    else if (mlen < 65536)
                    {
                        Assert.AreEqual(126, (int)(receivedBuffer[1] & 0x7F));
                        var sizeData = receivedBuffer.ToArrayData(2, 2);
                        Assert.AreEqual(mlen, (int)sizeData[0] * 256 + (int)sizeData[1]);
                        skip += 2;
                    }
                    else
                    {
                        Assert.AreEqual(127, (int)(receivedBuffer[1] & 0x7F));

                        var sizeData = receivedBuffer.ToArrayData(2, 8);

                        long len = 0;
                        int n = 1;

                        for (int k = 7; k >= 0; k--)
                        {
                            len += (int)sizeData[k] * n;
                            n *= 256;
                        }

                        Assert.AreEqual(mlen, len);
                        skip += 8;
                    }

                    Assert.AreEqual(sentMessages[j], Encoding.UTF8.GetString(receivedBuffer.ToArrayData(skip, mlen)));

                    receivedBuffer.ClearSegements();
                    Console.WriteLine("Passed " + j);
                }
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        //No mask
        private int SendMessage(Stream outputStream, int opCode, byte[] data)
        {
            byte[] playloadData = data;

            int length = playloadData.Length;

            byte[] headData;

            if (length < 126)
            {
                headData = new byte[6];
                headData[1] = (byte)(length | 0x80);
            }
            else if (length < 65536)
            {
                headData = new byte[8];
                headData[1] = (byte)(126 | 0x80);
                headData[2] = (byte)(length / 256);
                headData[3] = (byte)(length % 256);
            }
            else
            {
                headData = new byte[14];
                headData[1] = (byte)(127 | 0x80);

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    headData[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }

            headData[0] = (byte)(opCode | 0x80);

            GenerateMask(headData, headData.Length - 4);
            MaskData(playloadData, 0, playloadData.Length, headData, headData.Length - 4);

            outputStream.Write(headData, 0, headData.Length);
            outputStream.Write(playloadData, 0, playloadData.Length);

            return headData.Length + playloadData.Length - 4;
        }
    }
}

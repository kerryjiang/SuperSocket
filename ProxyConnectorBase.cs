using SuperSocket.Channel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client.Proxy
{
    public abstract class ProxyConnectorBase<TReceivePackage> : ConnectorBase where TReceivePackage : class
    {
        private EndPoint _proxyEndPoint;

        private IAsyncEnumerator<TReceivePackage> _packageEnumerator;

        protected EndPoint TargetEndPoint { get; private set; }

        protected string TargetHostHame { get; private set; }

        protected static Encoding ASCIIEncoding = new ASCIIEncoding();

        public ProxyConnectorBase(EndPoint proxyEndPoint) : this(proxyEndPoint, null)
        {
        }

        public ProxyConnectorBase(EndPoint proxyEndPoint, string targetHostHame)
        {
            _proxyEndPoint = proxyEndPoint;
            TargetHostHame = targetHostHame;
        }

        protected abstract ValueTask<bool> ProcessConnect(IChannel channel);

        protected abstract IChannel<TReceivePackage> GetPipeChannel(ConnectState state);

        protected async override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            TargetEndPoint = remoteEndPoint;//目标ip

            var proxyEndPoint = _proxyEndPoint; //代理ip

            IConnector socketConnector = new SocketConnector();

            ConnectState result;

            try
            {
                result = await socketConnector.ConnectAsync(proxyEndPoint, null, cancellationToken); //连接代理ip
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            if (!result.Result)
                return result;

            var conneced = false;
            Exception exception = null;
            var channel = GetPipeChannel(result);

            if (channel == null)
                throw new Exception("channel 不能为 null");

            _packageEnumerator = channel.RunAsync().GetAsyncEnumerator();

            try
            {
                conneced = await ProcessConnect(channel);
            }
            catch (Exception e)
            {
                exception = e;
                conneced = false;
            }

            try
            {
                await channel.DetachAsync();
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (!conneced && result.Socket.Connected) //连接失败 关闭 socket
            {
                try
                {
                    result.Socket.Close();
                }
                catch (Exception e)
                {
                    exception = e;
                }

                return new ConnectState
                {
                    Result = false,
                    Exception = exception
                };
            }

            return result;
        }

        protected byte[] GetEndPointBytes(EndPoint remoteEndPoint)
        {
            var targetEndPoint = remoteEndPoint;

            byte[] buffer;
            int actualLength;
            int port = 0;

            if (targetEndPoint is IPEndPoint)
            {
                var endPoint = targetEndPoint as IPEndPoint;
                port = endPoint.Port;

                if (endPoint.AddressFamily == AddressFamily.InterNetwork)
                {
                    buffer = new byte[10];
                    buffer[3] = 0x01;
                    Buffer.BlockCopy(endPoint.Address.GetAddressBytes(), 0, buffer, 4, 4);
                }
                else if (endPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    buffer = new byte[22];
                    buffer[3] = 0x04;

                    Buffer.BlockCopy(endPoint.Address.GetAddressBytes(), 0, buffer, 4, 16);
                }
                else
                {
                    throw new Exception("unknown address family");
                }

                actualLength = buffer.Length;
            }
            else
            {
                var endPoint = targetEndPoint as DnsEndPoint;

                port = endPoint.Port;

                var maxLen = 7 + ASCIIEncoding.GetMaxByteCount(endPoint.Host.Length);
                buffer = new byte[maxLen];

                buffer[3] = 0x03;
                buffer[4] = (byte)endPoint.Host.Length;//原来为0 现在为端口的长度
                actualLength = 5;
                actualLength += ASCIIEncoding.GetBytes(endPoint.Host, 0, endPoint.Host.Length, buffer, actualLength);
                actualLength += 2;
            }

            buffer[0] = 0x05;
            buffer[1] = 0x01;
            buffer[2] = 0x00;

            buffer[actualLength - 2] = (byte)(port / 256);
            buffer[actualLength - 1] = (byte)(port % 256);

            return buffer;
        }

        protected async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var enumerator = _packageEnumerator;

            if (await enumerator.MoveNextAsync())
                return enumerator.Current;

            return default;
        }
    }
}

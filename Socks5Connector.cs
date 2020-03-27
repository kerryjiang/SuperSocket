using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;

namespace SuperSocket.Client.Proxy
{
    public class Socks5Connector : ProxyConnectorBase<ProxyDataPackageInfo>
    {
        private ArraySegment<byte> _userNameAuthenRequest;

        readonly static byte[] _authenHandshake = new byte[] { 0x05, 0x02, 0x00, 0x02 };

        public Socks5Connector(EndPoint proxyEndPoint) : base(proxyEndPoint)
        {
        }

        public Socks5Connector(EndPoint proxyEndPoint, string username, string password)
         : base(proxyEndPoint)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("账号不能为空");

            var buffer = new byte[3 + ASCIIEncoding.GetMaxByteCount(username.Length) + (string.IsNullOrEmpty(password) ? 0 : ASCIIEncoding.GetMaxByteCount(password.Length))];
            var actualLength = 0;

            buffer[0] = 0x01;
            var len = ASCIIEncoding.GetBytes(username, 0, username.Length, buffer, 2);

            if (len > 255)
                throw new ArgumentException("账号长度不能超过255", "username");

            buffer[1] = (byte)len;

            actualLength = len + 2;

            if (!string.IsNullOrEmpty(password))
            {
                len = ASCIIEncoding.GetBytes(password, 0, password.Length, buffer, actualLength + 1);

                if (len > 255)
                    throw new ArgumentException("密码长度不能超过255", "password");

                buffer[actualLength] = (byte)len;
                actualLength += len + 1;
            }
            else
            {
                buffer[actualLength] = 0x00;
                actualLength++;
            }

            _userNameAuthenRequest = new ArraySegment<byte>(buffer, 0, actualLength);
        }

        protected override IChannel<ProxyDataPackageInfo> GetPipeChannel(ConnectState state)
        {
            return state.CreateChannel(new Socket5AuthenticateHandshakePipeFilter(), new ChannelOptions());
        }

        protected async override ValueTask<bool> ProcessConnect(IChannel channel)
        {
            if (await AuthenticateHandshake(channel))
            {
                if (!await AuthenticateUserName(channel))
                    return false;
            }

            return await AuthenticateEndPoint(channel);
        }

        /// <summary>
        /// 验证握手
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async ValueTask<bool> AuthenticateHandshake(IChannel channel)
        {
            await channel.SendAsync(_authenHandshake);

            var packageInfo = await base.ReceiveAsync();

            if (packageInfo == null || packageInfo.Data[1] == 255)
                throw new Exception($"连接代理失败,没有找到这个类型  { packageInfo.Data[1]} ");

            switch (packageInfo.Data[1])
            {
                case 0:
                    return false;  //无需密码
                case 2:
                    return true; //需要密码
                default:
                    throw new Exception($"连接代理失败,验证错误  { packageInfo.Data[1]} ");
            }
        }

        /// <summary>
        /// 验证账号密码
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async ValueTask<bool> AuthenticateUserName(IChannel channel)
        {
            await channel.SendAsync(_userNameAuthenRequest);

            var packageInfo = await base.ReceiveAsync();

            if (packageInfo == null || packageInfo.Data[1] == 255)
                throw new Exception($"连接代理失败,没有找到这个类型 { packageInfo.Data[1]} ");

            if (packageInfo.Data[1] != 0x00)
                throw new Exception($"连接代理失败,账号或密码错误  { packageInfo.Data[1]} ");

            return true;
        }

        /// <summary>
        /// 验证ip
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async ValueTask<bool> AuthenticateEndPoint(IChannel channel)
        {
            var handshakeBuffer = GetEndPointBytes(base.TargetEndPoint);

            await channel.SendAsync(handshakeBuffer);

            var packageInfo = await base.ReceiveAsync();

            if (packageInfo.Data[0] != 0x05)
                throw new Exception($"连接代理失败,协议版本不正确 { packageInfo.Data[0]} ");

            var status = packageInfo.Data[1];

            if (status == 0x00)
                return true;

            var errorMessage = string.Empty;

            switch (status)
            {
                case (0x02):
                    errorMessage = "connection not allowed by ruleset";
                    break;

                case (0x03):
                    errorMessage = "network unreachable";
                    break;

                case (0x04):
                    errorMessage = "host unreachable";
                    break;

                case (0x05):
                    errorMessage = "connection refused by destination host";
                    break;

                case (0x06):
                    errorMessage = "TTL expired";
                    break;

                case (0x07):
                    errorMessage = "command not supported / protocol error";
                    break;

                case (0x08):
                    errorMessage = "address type not supported";
                    break;

                default:
                    errorMessage = "general failure";
                    break;
            }

            throw new Exception($"连接代理失败,{ errorMessage } , { packageInfo.Data[0]} ");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.Common
{
    /// <summary>
    /// This class is designed for detect platform attribute in runtime
    /// </summary>
    public static class Platform
    {
        public static void Initialize()
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.IOControl(IOControlCode.KeepAliveValues, null, null);
                SupportSocketIOControlByCodeEnum = true;
            }
            catch (NotSupportedException)
            {
                SupportSocketIOControlByCodeEnum = false;
            }
            catch (NotImplementedException)
            {
                SupportSocketIOControlByCodeEnum = false;
            }
            catch (Exception)
            {
                SupportSocketIOControlByCodeEnum = true;
            }
        }

        public static bool SupportSocketIOControlByCodeEnum { get; private set; }
    }
}

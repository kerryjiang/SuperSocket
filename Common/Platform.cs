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
        static Platform()
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

            Type t = Type.GetType("Mono.Runtime");
            IsMono = t != null;
        }

        /// <summary>
        /// Gets a value indicating whether [support socket IO control by code enum].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [support socket IO control by code enum]; otherwise, <c>false</c>.
        /// </value>
        public static bool SupportSocketIOControlByCodeEnum { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is mono.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mono; otherwise, <c>false</c>.
        /// </value>
        public static bool IsMono { get; private set; }
    }
}

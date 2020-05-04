using System;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    public class HandshakeOptions
    {
        /// <summary>
        /// Handshake queue checking interval, in seconds
        /// </summary>
        /// <value>default: 60</value>
        public int CheckingInterval { get; set; } = 60;

        /// <summary>
        /// Open handshake timeout, in seconds
        /// </summary>
        /// <value>default: 120</value>
        public int OpenHandshakeTimeOut { get; set; } = 120;
        
        /// <summary>
        /// Close handshake timeout, in seconds
        /// </summary>
        /// <value>default: 120</value>
        public int CloseHandshakeTimeOut { get; set; } = 120;


        public Func<WebSocketSession, WebSocketPackage, ValueTask<bool>> HandshakeValidator { get; set; }
    }
}
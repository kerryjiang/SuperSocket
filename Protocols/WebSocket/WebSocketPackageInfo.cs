using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket protocol package instance
    /// </summary>
    public class WebSocketPackageInfo : StringPackageInfo
    {
        /// <summary>
        /// Gets the binary data.
        /// </summary>
        public IList<ArraySegment<byte>> BinaryData { get; private set; }

        /// <summary>
        /// Text message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketPackageInfo"/> class.
        /// </summary>
        public WebSocketPackageInfo(IList<ArraySegment<byte>> rawData, WebSocketContext context)
        {
                      
        }
    }
}

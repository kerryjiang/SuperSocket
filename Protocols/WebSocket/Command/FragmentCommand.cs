using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// FragmentCommand
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    abstract class FragmentCommand<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the UTF8 encoding which has been set ExceptionFallback.
        /// </summary>
        protected Encoding Utf8Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentCommand&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        public FragmentCommand()
        {
            Utf8Encoding = Encoding.GetEncoding(65001, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        }

        /// <summary>
        /// Checks the frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        protected bool CheckFrame(WebSocketDataFrame frame)
        {
            //Check RSV
            return (frame.InnerData[0] & 0x70) == 0x00;
        }

        /// <summary>
        /// Checks the control frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        protected bool CheckControlFrame(WebSocketDataFrame frame)
        {
            if (!CheckFrame(frame))
                return false;

            //http://tools.ietf.org/html/rfc6455#section-5.5
            //All control frames MUST have a payload length of 125 bytes or less and MUST NOT be fragmented
            if (!frame.FIN || frame.ActualPayloadLength > 125)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets data from websocket frames.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <returns></returns>
        protected byte[] GetWebSocketData(IList<WebSocketDataFrame> frames)
        {
            int offset, length;

            var resultBuffer = new byte[frames.Sum(f => (int)f.ActualPayloadLength)];

            int copied = 0;

            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                length = (int)frame.ActualPayloadLength;

                if (length > 0)
                {
                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    frame.InnerData.CopyTo(resultBuffer, offset, copied, length);
                    copied += length;
                }
            }

            return resultBuffer;
        }

        /// <summary>
        /// Gets text string from websocket frames.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <returns></returns>
        protected string GetWebSocketText(IList<WebSocketDataFrame> frames)
        {
            var data = GetWebSocketData(frames);
            return Utf8Encoding.GetString(data);
        }

        /// <summary>
        /// Gets data from a websocket frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        protected byte[] GetWebSocketData(WebSocketDataFrame frame)
        {
            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
            int length = (int)frame.ActualPayloadLength;

            if (frame.HasMask && length > 0)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            byte[] data;

            if (length > 0)
                data = frame.InnerData.ToArrayData(offset, length);
            else
                data = new byte[0];

            return data;
        }

        /// <summary>
        /// Gets text string from a websocket frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        protected string GetWebSocketText(WebSocketDataFrame frame)
        {
            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
            int length = (int)frame.ActualPayloadLength;

            if (frame.HasMask && length > 0)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            string text;

            if (length > 0)
            {
                text = frame.InnerData.Decode(Utf8Encoding, offset, length);
            }
            else
            {
                text = string.Empty;
            }

            return text;
        }
    }
}

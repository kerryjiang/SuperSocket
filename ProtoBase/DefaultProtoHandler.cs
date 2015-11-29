using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Default implementation of ProtoHandler
    /// </summary>
    public class DefaultProtoHandler : ProtoHandlerBase
    {
        /// <summary>
        /// Determines whether this instance can send.
        /// </summary>
        /// <returns></returns>
        public override bool CanSend()
        {
            return true;
        }

        /// <summary>
        /// Closes the specified channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="reason">The reason.</param>
        public override void Close(ICommunicationChannel channel, CloseReason reason)
        {
            channel.Close(reason);
        }
    }
}

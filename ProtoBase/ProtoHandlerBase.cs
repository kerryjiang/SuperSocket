using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// ProtoHandler's base class
    /// </summary>
    public abstract class ProtoHandlerBase : IProtoHandler
    {
        /// <summary>
        /// Gets the binary data encoder.
        /// </summary>
        /// <value>
        /// The binary data encoder.
        /// </value>
        public IProtoDataEncoder DataEncoder { get; set; }


        /// <summary>
        /// Determines whether this instance can send.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanSend();

        /// <summary>
        /// Closes the specified channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="reason">The reason.</param>
        public abstract void Close(ICommunicationChannel channel, CloseReason reason);
    }
}

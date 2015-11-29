using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// the protocol handler interface
    /// </summary>
    public interface IProtoHandler
    {
        /// <summary>
        /// Determines whether this instance can send.
        /// </summary>
        /// <returns></returns>
        bool CanSend();

        /// <summary>
        /// Gets the data encoder.
        /// </summary>
        /// <value>
        /// The data encoder.
        /// </value>
        IProtoDataEncoder DataEncoder { get; }


        /// <summary>
        /// Closes the specified channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="reason">The reason.</param>
        void Close(ICommunicationChannel channel, CloseReason reason);
    }
}

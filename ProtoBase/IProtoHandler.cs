using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets the binary data encoder.
        /// </summary>
        /// <value>
        /// The binary data encoder.
        /// </value>
        IProtoDataEncoder DataEncoder { get; }

        /// <summary>
        /// Gets the text encoder.
        /// </summary>
        /// <value>
        /// The text encoder.
        /// </value>
        IProtoTextEncoder TextEncoder { get; }


        /// <summary>
        /// Closes the specified channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="reason">The reason.</param>
        void Close(ICommunicationChannel channel, CloseReason reason);
    }
}

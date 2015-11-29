using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for communication channel
    /// </summary>
    public interface ICommunicationChannel
    {
        /// <summary>
        /// Send the binary segment to the other endpoint through this communication channel
        /// </summary>
        /// <param name="segment">the data segment to be sent</param>
        void Send(ArraySegment<byte> segment);

        /// <summary>
        /// Close the communication channel
        /// </summary>
        /// <param name="reason">The reason.</param>
        void Close(CloseReason reason);
    }
}

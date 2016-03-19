using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for protocol encoder of text messages
    /// </summary>
    public interface IProtoTextEncoder
    {
        /// <summary>
        /// Encode text message
        /// </summary>
        /// <param name="output">the output buffer</param>
        /// <param name="message">the message to be encoded</param>
        /// <returns>the output binary data</returns>
        void EncodeText(IOutputBuffer output, string message);
    }
}

using System;
using Google.Protobuf;

namespace SuperSocket.ProtoBuf
{
    public class ProtobufPackageInfo
    {
        /// <summary>
        /// Gets or sets the parsed protobuf message
        /// </summary>
        public IMessage Message { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the message
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// Gets or sets the type identifier of the message
        /// </summary>
        public int TypeId { get; set; }
    }
}
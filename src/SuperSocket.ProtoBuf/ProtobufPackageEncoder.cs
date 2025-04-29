using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace SuperSocket.ProtoBuf
{
    public class ProtobufPackageEncoder : IPackageEncoder<ProtobufPackageInfo>
    {
        private readonly Dictionary<Type, int> _typeToIdMapping = new Dictionary<Type, int>();

        /// <summary>
        /// Register a message type with its type identifier
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="typeId">The message type identifier</param>
        public void RegisterMessageType(Type messageType, int typeId)
        {
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            _typeToIdMapping[messageType] = typeId;
        }

        public int Encode(IBufferWriter<byte> writer, ProtobufPackageInfo package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (package.Message == null)
                throw new ArgumentException("Message cannot be null", nameof(package));

            var message = package.Message;
            var messageType = message.GetType();

            // Get the message type ID from the package directly or from the mapping
            int typeId = package.TypeId;
            
            // If type ID is not set, try to get it from the mapping
            if (typeId == 0)
            {
                if (!_typeToIdMapping.TryGetValue(messageType, out typeId))
                    throw new InvalidOperationException($"Message type {messageType.FullName} is not registered with a type ID");
            }

            // Calculate the message size
            int messageSize = message.CalculateSize();
            
            // Reserve space for the header (length + type ID)
            var headerBuffer = writer.GetSpan(8);
            
            // Write the message size (excluding the type ID field)
            BinaryPrimitives.WriteInt32BigEndian(headerBuffer, messageSize);
            
            // Write the message type ID
            BinaryPrimitives.WriteInt32BigEndian(headerBuffer.Slice(4), typeId);
            
            // Advance the writer past the header
            writer.Advance(8);

            // Write the actual message
            var messageBuffer = writer.GetSpan(messageSize);
            message.WriteTo(messageBuffer);
            writer.Advance(messageSize);

            // Return the total bytes written
            return messageSize + 8;
        }
    }
}
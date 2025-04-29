using System;
using System.Buffers;
using System.Collections.Generic;
using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace SuperSocket.ProtoBuf
{
    public class ProtobufPipelineFilter : FixedHeaderPipelineFilter<ProtobufPackageInfo>
    {
        private readonly Dictionary<int, MessageParser> _parsers = new Dictionary<int, MessageParser>();
        private readonly Dictionary<int, Type> _messageTypes = new Dictionary<int, Type>();

        public ProtobufPipelineFilter()
            : base(8) // 4 bytes for message length + 4 bytes for message type identifier
        {
        }

        /// <summary>
        /// Register a message type with its type identifier
        /// </summary>
        /// <param name="typeId">The message type identifier</param>
        /// <param name="parser">The protobuf message parser</param>
        /// <param name="messageType">The message type</param>
        public void RegisterMessageType(int typeId, MessageParser parser, Type messageType)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
                
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            _parsers[typeId] = parser;
            _messageTypes[typeId] = messageType;
        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            reader.TryReadBigEndian(out int bodyLength);
            return bodyLength;
        }

        protected override ProtobufPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            
            // Skip the length field that we already processed
            reader.Advance(4);
            
            // Read the message type identifier
            reader.TryReadBigEndian(out int messageTypeId);

            // Get the appropriate parser and message type
            if (!_parsers.TryGetValue(messageTypeId, out var parser))
                throw new ProtocolException($"No message parser registered for type id: {messageTypeId}");

            if (!_messageTypes.TryGetValue(messageTypeId, out var messageType))
                throw new ProtocolException($"No message type registered for type id: {messageTypeId}");

            // Use the remaining buffer (actual protobuf message data)
            var messageBuffer = buffer.Slice(8);
            var message = parser.ParseFrom(messageBuffer);
            
            return new ProtobufPackageInfo
            {
                Message = message,
                MessageType = messageType,
                TypeId = messageTypeId
            };
        }
    }
}
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    public class WebSocketEncoder : IPackageEncoder<WebSocketPackage>
    {
        private static readonly Encoding _textEncoding = new UTF8Encoding(false);

        private const int _size0 = 126;
        private const int _size1 = 65536;

        private readonly int[] _fragmentSizes;

        private readonly ArrayPool<byte> _bufferPool;

        protected ArrayPool<byte> BufferPool => _bufferPool;
        
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }

        private static int[] _defaultFragmentSizes = new int []
            {
                1024,
                1024 * 4,
                1024 * 8,
                1024 * 16,
                1024 * 32,
                1024 * 64
            };

        public WebSocketEncoder()
            : this(ArrayPool<byte>.Shared, _defaultFragmentSizes)
        {
        }

        public WebSocketEncoder(ArrayPool<byte> bufferPool, int[] fragmentSizes)
        {
            _bufferPool = bufferPool;
            _fragmentSizes = fragmentSizes;
        }

        protected virtual int WriteHead(IBufferWriter<byte> writer, byte opCode, long length)
        {
            var head = WriteHead(writer, opCode, length, out var headLen);
            head[0] = opCode;
            writer.Advance(headLen);
            return headLen;
        }

        private Span<byte> WriteHead(IBufferWriter<byte> writer, byte opCode, long length, out int headLen)
        {
            if (length < _size0)
            {
                headLen = 2;
                var head = writer.GetSpan(headLen);
                head[1] = (byte)length;

                return head;
            }
            else if (length < _size1)
            {
                headLen = 4;

                var head = writer.GetSpan(headLen);

                head[1] = (byte)_size0;
                head[2] = (byte)(length / 256);
                head[3] = (byte)(length % 256);
                return head;
            }
            else
            {
                headLen = 10;

                var head = writer.GetSpan(headLen);

                head[1] = (byte)127;

                long left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    if (left == 0)
                    {
                        head[i] = 0;
                        continue;
                    }

                    head[i] = (byte)(left % unit);
                    left = left / unit;
                }

                return head;
            }
        }

        private int EncodeEmptyFragment(IBufferWriter<byte> writer, byte opCode)
        {
            return EncodeFinalFragment(writer, opCode, ReadOnlySpan<char>.Empty, null, out var _);
        }

        private int EncodeFragment(IBufferWriter<byte> writer, byte opCode, int fragmentSize, ReadOnlySpan<char> text, Encoder encoder, out int charsUsed)
        {
            charsUsed = 0;

            var headLen = WriteHead(writer, opCode, fragmentSize);

            var encodingContext = CreateDataEncodingContext(writer);

            OnHeadEncoded(writer, encodingContext);

            var totalBytes = 0;
            var dataSizeToWrite = fragmentSize;

            while (dataSizeToWrite > 0)
            {
                var buffer = writer.GetSpan(dataSizeToWrite);
                var bufferToWrite = Math.Min(buffer.Length, dataSizeToWrite);
                buffer = buffer[..bufferToWrite];

                encoder.Convert(text, buffer, false, out charsUsed, out var bytesUsed, out var completed);

                buffer = buffer[..bytesUsed];

                OnDataEncoded(buffer, encodingContext, totalBytes);
                writer.Advance(bytesUsed);

                totalBytes += bytesUsed;

                dataSizeToWrite -= charsUsed;

                if (totalBytes > fragmentSize)
                {
                    throw new Exception("Size of the data from the decoding must be equal to the fragment size.");
                }
            }

            return GetFragmentTotalLength(headLen, totalBytes);
        }

        protected virtual object CreateDataEncodingContext(IBufferWriter<byte> writer)
        {
            return null;
        }

        protected virtual void OnHeadEncoded(IBufferWriter<byte> writer, object encodingContext)
        {
        }

        protected virtual void OnDataEncoded(Span<byte> encodedData, object encodingContext, int previusEncodedDataSize)
        {
        }

        protected virtual void CleanupEncodingContext(object encodingContext)
        {
        }

        protected virtual int GetFragmentTotalLength(int headLen, int bodyLen)
        {
            return headLen + bodyLen;
        }

        private int EncodeFinalFragment(IBufferWriter<byte> writer, byte opCode, ReadOnlySpan<char> text, Encoder encoder, out int charsUsed)
        {
            charsUsed = 0;

            byte[] buffer = default;

            object encodingContext = default;

            try
            {
                // writer should not be touched for now, because head has not been written yet.
                encodingContext = CreateDataEncodingContext(null);

                var bytesUsed = 0;
                Span<byte> bufferSpan = default;

                if (text.Length > 0)
                {
                    var bufferSize = text.Length <= _size0 ? _size0 : _fragmentSizes[0];

                    buffer = _bufferPool.Rent(bufferSize);

                    bufferSpan = buffer.AsSpan();

                    encoder.Convert(text, bufferSpan, false, out charsUsed, out bytesUsed, out bool completed);

                    OnDataEncoded(bufferSpan[..bytesUsed], encodingContext, 0);

                    var isFinal = completed && text.Length == charsUsed;

                    if (isFinal)
                        opCode = (byte)(opCode | 0x80);
                }
                else
                {
                    opCode = (byte)(opCode | 0x80);
                    bytesUsed = 0;
                }

                var headLen = WriteHead(writer, opCode, bytesUsed);

                OnHeadEncoded(writer, encodingContext);

                if (bytesUsed > 0)
                {
                    writer.Write(bufferSpan[..bytesUsed]);
                }

                return GetFragmentTotalLength(headLen, bytesUsed);
            }
            finally
            {
                if (buffer != null)
                    _bufferPool.Return(buffer);
                
                CleanupEncodingContext(encodingContext);
            }
        }

        protected virtual void EncodeDataMessageBody(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            foreach (var dataPiece in pack.Data)
            {
                writer.Write(dataPiece.Span);
            }
        }

        public int EncodeDataMessage(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            var headLen = WriteHead(writer, (byte)(pack.OpCodeByte | 0x80), pack.Data.Length);

            EncodeDataMessageBody(writer, pack);

            return (int)(pack.Data.Length + headLen);
        }

        private int GetFragmentSize(int msgSize)
        {
            for (var i = _fragmentSizes.Length - 1; i >= 0; i--)
            {
                var fragmentSize = _fragmentSizes[i];
                
                if (msgSize >= fragmentSize)
                {
                    return fragmentSize;
                }
            }

            return 0;
        }
        
        public int Encode(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            pack.SaveOpCodeByte();

            var extensions = Extensions;

            if (extensions != null && extensions.Count > 0)
            {
                foreach (var ext in extensions)
                {
                    ext.Encode(pack);
                }
            }

            if (!pack.Data.IsEmpty)
                return EncodeDataMessage(writer, pack);

            var msgSize = !string.IsNullOrEmpty(pack.Message) ? pack.Message.Length : 0;

            if (msgSize == 0)
                return EncodeEmptyFragment(writer, pack.OpCodeByte);

            var total = 0;
            var text = pack.Message.AsSpan();

            var encoder = _textEncoding.GetEncoder();

            var isContinuation = false;

            while (true)
            {
                var fragmentSize = GetFragmentSize(text.Length);

                var charsUsed = 0;

                if (fragmentSize > 0)
                {
                    total += EncodeFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, fragmentSize, text, encoder, out charsUsed);
                }                    
                else
                {
                    total += EncodeFinalFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, text, encoder, out charsUsed);
                    
                    if (text.Length <= charsUsed)
                        break;
                }

                text = text.Slice(charsUsed);
                
                if (!isContinuation)
                    isContinuation = true;
            }
            
            return total;
        }
    }
}
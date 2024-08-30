using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    public class WebSocketEncoder : IPackageEncoder<WebSocketPackage>
    {
        private static readonly Encoding _textEncoding = new UTF8Encoding(false);

        private static readonly int _minEncodeBufferSize; 

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

        static WebSocketEncoder()
        {
            _minEncodeBufferSize = _textEncoding.GetMaxByteCount(1);
        }

        public WebSocketEncoder()
            : this(ArrayPool<byte>.Shared, _defaultFragmentSizes)
        {
        }

        public WebSocketEncoder(ArrayPool<byte> bufferPool, int[] fragmentSizes)
        {
            _bufferPool = bufferPool;
            _fragmentSizes = fragmentSizes;

            if (fragmentSizes.Any(size => size <= _minEncodeBufferSize))
            {
                throw new ArgumentException($"fragmentSize should be larger than {_minEncodeBufferSize}.", nameof(fragmentSizes));
            }
        }

        private int WriteHead(IBufferWriter<byte> writer, byte opCode, long length)
        {
            var head = WriteHead(writer, length, out var headLen);
            head[0] = opCode;
            writer.Advance(headLen);
            return headLen;
        }

        protected virtual Span<byte> WriteHead(IBufferWriter<byte> writer, long length, out int headLen)
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
            return EncodeFinalFragment(writer, opCode, ReadOnlySpan<char>.Empty, null, default);
        }

        private int EncodeFragment(IBufferWriter<byte> writer, byte opCode, int fragmentSize, ReadOnlySpan<char> text, Encoder encoder, ref ArraySegment<byte> unwrittenBytes, out int charsUsed)
        {
            charsUsed = 0;

            var headLen = WriteHead(writer, opCode, fragmentSize);

            var encodingContext = CreateDataEncodingContext(writer);

            OnHeadEncoded(writer, encodingContext);

            var totalBytes = 0;
            var dataSizeToWrite = fragmentSize;

            var unwrittenBytesLen = unwrittenBytes.Count;

            if (unwrittenBytesLen > 0)
            {
                var spanToWrite = writer.GetSpan(unwrittenBytesLen);
                unwrittenBytes.AsSpan().CopyTo(spanToWrite);

                OnDataEncoded(spanToWrite.Slice(0, unwrittenBytesLen), encodingContext, 0);
                writer.Advance(unwrittenBytesLen);

                totalBytes += unwrittenBytesLen;
                dataSizeToWrite -= unwrittenBytesLen;

                unwrittenBytes = unwrittenBytes.Slice(0, 0);
            }

            while (true)
            {
                var encodeBufferSize = Math.Max(dataSizeToWrite, _minEncodeBufferSize);
                var buffer = writer.GetSpan(encodeBufferSize);
                buffer = buffer[..encodeBufferSize];

                var bytesUsed = 0;
                var convertCharsUsed = 0;

                try
                {
                    encoder.Convert(text, buffer, false, out convertCharsUsed, out bytesUsed, out var completed);
                }
                catch (ArgumentException ex)
                {
                    throw new Exception($"textToEncode: {text.Length}, buffer: {buffer.Length}.", ex);
                }

                // We get more data than what we expect, save unwritten bytes.
                if (bytesUsed > dataSizeToWrite)
                {
                    var more = bytesUsed - dataSizeToWrite;

                    if (more > 0)
                    {
                        var unwrittenBytesBuffer = unwrittenBytes.Array ?? _bufferPool.Rent(_minEncodeBufferSize);
                        buffer.Slice(dataSizeToWrite, more).CopyTo(unwrittenBytesBuffer.AsSpan(0, more));
                        unwrittenBytes = new ArraySegment<byte>(unwrittenBytesBuffer, 0, more);
                    }

                    bytesUsed = dataSizeToWrite;
                }   

                buffer = buffer[..bytesUsed];

                // totalBytes here is previous encoded data size
                OnDataEncoded(buffer, encodingContext, totalBytes);
                writer.Advance(bytesUsed);

                totalBytes += bytesUsed;
                charsUsed += convertCharsUsed;

                dataSizeToWrite -= bytesUsed;

                if (dataSizeToWrite == 0)
                    break;

                text = text.Slice(convertCharsUsed);

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

        private int EncodeFinalFragment(IBufferWriter<byte> writer, byte opCode, ReadOnlySpan<char> text, Encoder encoder, ArraySegment<byte> unwrittenBytes)
        {
            byte[] buffer = default;

            object encodingContext = default;

            try
            {
                // writer should not be touched for now, because head has not been written yet.
                encodingContext = CreateDataEncodingContext(null);
                
                var totalWritten = 0;

                Span<byte> bufferSpan = default;

                var fragementSize = (text.Length > 0 ? encoder.GetByteCount(text, true) : 0) + unwrittenBytes.Count;

                if (fragementSize == 0)
                    fragementSize = _minEncodeBufferSize;

                buffer = _bufferPool.Rent(fragementSize);

                bufferSpan = buffer.AsSpan();

                if (unwrittenBytes.Count > 0)
                {
                    unwrittenBytes.AsSpan().CopyTo(bufferSpan);
                    totalWritten += unwrittenBytes.Count;
                    OnDataEncoded(bufferSpan.Slice(0, unwrittenBytes.Count), encodingContext, 0);
                }

                encoder.Convert(text, totalWritten == 0 ? bufferSpan : bufferSpan.Slice(totalWritten), true, out var charsUsed, out var bytesUsed, out bool completed);

                OnDataEncoded(bufferSpan.Slice(totalWritten, bytesUsed), encodingContext, totalWritten);

                totalWritten += bytesUsed;

                if (!completed || text.Length != charsUsed)
                {
                    throw new ProtocolException("Unexpected encoding behavior: the text encoding didn't complete with enough buffer.");
                }

                opCode = (byte)(opCode | 0x80);

                var headLen = WriteHead(writer, opCode, totalWritten);

                OnHeadEncoded(writer, encodingContext);

                if (totalWritten > 0)
                {
                    writer.Write(bufferSpan[..totalWritten]);
                }

                return GetFragmentTotalLength(headLen, totalWritten);
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

            ArraySegment<byte> unwritteBytes = default;

            while (true)
            {
                var fragmentSize = GetFragmentSize(text.Length + unwritteBytes.Count);

                if (fragmentSize <= 0)
                {
                    try
                    {
                        total += EncodeFinalFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, text, encoder, unwritteBytes);
                    }
                    finally
                    {
                        if (unwritteBytes.Count > 0)
                            _bufferPool.Return(unwritteBytes.Array);
                    }
                    
                    break;
                }

                total += EncodeFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, fragmentSize, text, encoder, ref unwritteBytes, out var charsUsed);
                text = text.Slice(charsUsed);
                
                if (!isContinuation)
                    isContinuation = true;
            }
            
            return total;
        }
    }
}
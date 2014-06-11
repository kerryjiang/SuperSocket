using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The reader interface for the data source in the type of IList{ArraySegment{byte}}
    /// </summary>
    public interface IBufferReader : IDisposable
    {
        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        long Length { get; }

        /// <summary>
        /// Initializes the reader with the data source segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        void Initialize(IList<ArraySegment<byte>> segments);

        /// <summary>
        /// Resets this reader.
        /// </summary>
        void Reset();

        /// <summary>
        /// Reads a Int16 number from the current data source.
        /// </summary>
        /// <returns></returns>
        Int16 ReadInt16();

        /// <summary>
        /// Reads a Int16 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        Int16 ReadInt16(bool littleEndian);

        /// <summary>
        /// Reads a UInt16 number from the current data source.
        /// </summary>
        /// <returns></returns>
        UInt16 ReadUInt16();

        /// <summary>
        /// Reads a UInt16 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        UInt16 ReadUInt16(bool littleEndian);

        /// <summary>
        /// Skips the specified count bytes from the data source.
        /// </summary>
        /// <param name="count">The count.</param>
        IBufferReader Skip(int count);

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <returns></returns>
        Int32 ReadInt32();

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        Int32 ReadInt32(bool littleEndian);

        /// <summary>
        /// Reads a UInt32 number from the current data source.
        /// </summary>
        /// <returns></returns>
        UInt32 ReadUInt32();

        /// <summary>
        /// Reads a UInt32 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        UInt32 ReadUInt32(bool littleEndian);

        /// <summary>
        /// Reads a Int64 number from the current data source.
        /// </summary>
        /// <returns></returns>
        Int64 ReadInt64();

        /// <summary>
        /// Reads a Int64 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        Int64 ReadInt64(bool littleEndian);

        /// <summary>
        /// Reads a UInt64 number from the current data source.
        /// </summary>
        /// <returns></returns>
        UInt64 ReadUInt64();

        /// <summary>
        /// Reads a UInt64 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        UInt64 ReadUInt64(bool littleEndian);

        /// <summary>
        /// Reads a byte from the data source
        /// </summary>
        /// <returns></returns>
        byte ReadByte();

        /// <summary>
        /// Reads many bytes from the current data source.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        int ReadBytes(byte[] output, int offset, int count);

        /// <summary>
        /// Reads a string from the current data source
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        string ReadString(int length, Encoding encoding);


        /// <summary>
        /// Takes the data of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        IList<ArraySegment<byte>> Take(int length);
    }

    /// <summary>
    /// The default buffer list reader
    /// </summary>
    public class BufferListReader : IBufferReader
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length
        {
            get { return m_Length; }
        }

        private byte[] m_Buffer = new byte[8];

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferListReader"/> class.
        /// </summary>
        public BufferListReader()
        {

        }

        /// <summary>
        /// Initializes the reader with the data source segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <exception cref="System.ArgumentException">The length of segments must be greater than zero.</exception>
        public void Initialize(IList<ArraySegment<byte>> segments)
        {
            if (segments.Count <= 0)
                throw new ArgumentException("The length of segments must be greater than zero.");

            m_Segments = segments;
            m_CurrentSegmentOffset = segments[0].Offset;

            long length = 0;

            for (var i = 0; i < segments.Count; i++)
            {
                length += segments[i].Count;
            }

            m_Length = length;
        }

        private const string c_ThreadBufferSegmentReader = "ThreadBufferListReader";

        /// <summary>
        /// Gets the current buffer reader from the thread context
        /// </summary>
        /// <returns></returns>
        public static IBufferReader GetCurrent()
        {
            return GetCurrent<BufferListReader>();
        }

        /// <summary>
        /// Gets the current buffer reader from the thread context
        /// </summary>
        /// <typeparam name="TReader">The type of the reader.</typeparam>
        /// <returns></returns>
        public static IBufferReader GetCurrent<TReader>()
            where TReader : IBufferReader, new()
        {
            var slot = Thread.GetNamedDataSlot(c_ThreadBufferSegmentReader);
            var reader = Thread.GetData(slot) as IBufferReader;
            if (reader != null)
                return reader;

            reader = new TReader();
            Thread.SetData(slot, reader);
            return reader;
        }

        /// <summary>
        /// Resets this reader.
        /// </summary>
        public void Reset()
        {
            m_Segments = null;
            m_CurrentSegmentIndex = 0;
            m_CurrentSegmentOffset = 0;
            m_Length = 0;
            m_Position = 0;
        }

        private int Read(byte[] buffer, int offset, int count)
        {
            if (m_Position == m_Length)
                throw new Exception("Reach the end of the data source");

            var currentOffset = m_CurrentSegmentOffset;
            var len = 0;
            var segIndex = 0;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];

                if (i != m_CurrentSegmentIndex)
                    currentOffset = segment.Offset;

                var rest = count - len;
                var segLen = segment.Count - (currentOffset - segment.Offset);
                var thisLen = Math.Min(segLen, rest);

                Buffer.BlockCopy(segment.Array, currentOffset, buffer, offset + len, thisLen);

                len += thisLen;
                segIndex = i;

                if (len >= count)
                {
                    if (segLen > rest)
                    {
                        m_CurrentSegmentIndex = i;
                        m_CurrentSegmentOffset = currentOffset + thisLen;
                    }
                    else //segLen == rest
                    {
                        var nextSegIndex = i + 1;
                        if (nextSegIndex >= m_Segments.Count)
                        {
                            m_CurrentSegmentIndex = i;
                            m_CurrentSegmentOffset = currentOffset + thisLen;
                        }
                        else
                        {
                            m_CurrentSegmentIndex = nextSegIndex;
                            m_CurrentSegmentOffset = m_Segments[nextSegIndex].Offset;
                        }
                    }

                    m_Position += len;
                    return len;
                }
            }

            //No enougth data to read
            m_CurrentSegmentIndex = segIndex;
            m_CurrentSegmentOffset = 0;
            m_Position = m_Length;

            return len;
        }

        /// <summary>
        /// Fills the buffer.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// length;the length must between 1 and 8
        /// or
        /// length;there is no enough data to read
        /// </exception>
        protected void FillBuffer(int length)
        {
            if (length > 8)
                throw new ArgumentOutOfRangeException("length", "the length must between 1 and 8");

            var read = Read(m_Buffer, 0, length);

            if(read != length)
                throw new ArgumentOutOfRangeException("length", "there is no enough data to read");
        }

        /// <summary>
        /// Reads a Int16 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            return ReadInt16(false);
        }

        /// <summary>
        /// Reads a Int16 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public short ReadInt16(bool littleEndian)
        {
            FillBuffer(2);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (short)((int)buffer[0] | (int)buffer[1] << 8);
            else
                return (short)((int)buffer[1] | (int)buffer[0] << 8);
        }

        /// <summary>
        /// Reads a UInt16 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            return ReadUInt16(false);
        }

        /// <summary>
        /// Reads a UInt16 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public ushort ReadUInt16(bool littleEndian)
        {
            FillBuffer(2);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (ushort)((int)buffer[0] | (int)buffer[1] << 8);
            else
                return (ushort)((int)buffer[1] | (int)buffer[0] << 8);
        }

        /// <summary>
        /// Skips the specified count bytes from the data source.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// count;count cannot be zero or negative
        /// or
        /// count;exceed the total length
        /// </exception>
        public IBufferReader Skip(int count)
        {
            if(count <= 0)
                throw new ArgumentOutOfRangeException("count", "count cannot be zero or negative");

            var pos = m_Position + count;
            
            if(pos >= m_Length)
                throw new ArgumentOutOfRangeException("count", "exceed the total length");


            var currentOffset = m_CurrentSegmentOffset;
            var rest = count;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];
                if (i != m_CurrentSegmentIndex)
                    currentOffset = segment.Offset;

                var thisLen = segment.Count - (currentOffset - segment.Offset);

                if (rest < thisLen)
                {
                    m_CurrentSegmentIndex = i;
                    m_CurrentSegmentOffset = segment.Offset + segment.Count - rest;
                    break;
                }
                
                if (rest > thisLen)
                {
                    rest -= thisLen;
                    continue;
                }

                // rest == thisLen
                var nextSegmentIndex = i + 1;

                if (nextSegmentIndex < m_Segments.Count)
                {
                    m_CurrentSegmentIndex = nextSegmentIndex;
                    m_CurrentSegmentOffset = m_Segments[nextSegmentIndex].Offset;
                }
                else
                {
                    m_CurrentSegmentIndex = i;
                    m_CurrentSegmentOffset = 0;
                }

                break;
            }

            m_Position = pos;
            return this;
        }

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            return ReadInt32(false);
        }

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public int ReadInt32(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (int)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
            else
                return (int)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
        }

        /// <summary>
        /// Reads a UInt32 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            return ReadUInt32(false);
        }

        /// <summary>
        /// Reads a UInt32 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public uint ReadUInt32(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
            else
                return (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
        }

        /// <summary>
        /// Reads a Int64 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            return ReadInt64(false);
        }

        /// <summary>
        /// Reads a Int64 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public long ReadInt64(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
            {
                var num = (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
                var num2 = (uint)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24);
                return (long)((ulong)num2 << 32 | (ulong)num);
            }
            else
            {
                var num = (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
                var num2 = (uint)((int)buffer[7] | (int)buffer[6] << 8 | (int)buffer[5] << 16 | (int)buffer[4] << 24);
                return (long)((ulong)num << 32 | (ulong)num2);
            }
        }

        /// <summary>
        /// Reads a UInt64 number from the current data source.
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            return ReadUInt64(false);
        }

        /// <summary>
        /// Reads a UInt64 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> [little endian].</param>
        /// <returns></returns>
        public ulong ReadUInt64(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
            {
                var num = (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
                var num2 = (uint)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24);
                return ((ulong)num2 << 32 | (ulong)num);
            }
            else
            {
                var num = (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
                var num2 = (uint)((int)buffer[7] | (int)buffer[6] << 8 | (int)buffer[5] << 16 | (int)buffer[4] << 24);
                return ((ulong)num << 32 | (ulong)num2);
            }
        }

        /// <summary>
        /// Reads a byte from the data source
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Reach the end of the data source</exception>
        public byte ReadByte()
        {
            if (m_Position == m_Length)
                throw new Exception("Reach the end of the data source");

            var currentSegment = m_Segments[m_CurrentSegmentIndex];
            var targetByte = currentSegment.Array[m_CurrentSegmentOffset];
            var nextOffset = m_CurrentSegmentOffset + 1;
            var maxOffset = currentSegment.Offset + currentSegment.Count - 1;

            m_Position++;

            if (nextOffset <= maxOffset) // next pos is within the current segment
            {
                m_CurrentSegmentOffset = nextOffset;
                return targetByte;
            }

            // next pos is within the next segment
            var nextSegmentIndex = m_CurrentSegmentIndex + 1;

            if (nextSegmentIndex < m_Segments.Count)
            {
                m_CurrentSegmentIndex = nextSegmentIndex;
                m_CurrentSegmentOffset = m_Segments[nextSegmentIndex].Offset;
            }

            return targetByte;
        }

        /// <summary>
        /// Reads many bytes from the current data source.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public int ReadBytes(byte[] output, int offset, int count)
        {
            return Read(output, offset, count);
        }

        /// <summary>
        /// Reads a string from the current data source
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">length;there is no enougth data</exception>
        public string ReadString(int length, Encoding encoding)
        {
            var output = new char[encoding.GetMaxCharCount(length)];

            var decoder = encoding.GetDecoder();

            var totalCharsLen = 0;
            var totalBytesLen = 0;
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;
            var rest = length;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];
                var srcOffset = segment.Offset;
                var srcLength = segment.Count;

                if (i == m_CurrentSegmentIndex)
                {
                    srcOffset = m_CurrentSegmentOffset;
                    srcLength = segment.Offset + segment.Count - srcOffset;
                }

                var thisLength = Math.Min(rest, srcLength);
                rest -= thisLength;

                var lastSegment = rest <= 0;

                decoder.Convert(segment.Array, srcOffset, thisLength, output, totalCharsLen, output.Length - totalCharsLen, lastSegment, out bytesUsed, out charsUsed, out completed);
                totalCharsLen += charsUsed;
                totalBytesLen += bytesUsed;

                if (lastSegment)
                    break;
            }

            if (rest > 0)
                throw new ArgumentOutOfRangeException("length", "there is no enougth data");

            return new string(output, 0, totalCharsLen);
        }

        private IList<ArraySegment<byte>> Clone(int index, int segmentOffset, int length)
        {
            var target = new List<ArraySegment<byte>>();

            var rest = length;

            var segments = m_Segments;

            for (var i = index; i < segments.Count; i++)
            {
                var segment = segments[i];
                var offset = segment.Offset;
                var thisLen = segment.Count;

                if (i == index)
                {
                    offset = segmentOffset;
                    thisLen = segment.Count - (segmentOffset - segment.Offset);
                }

                thisLen = Math.Min(thisLen, rest);

                target.Add(new ArraySegment<byte>(segment.Array, offset, thisLen));

                rest -= thisLen;

                if (rest <= 0)
                    break;
            }

            return target;
        }

        /// <summary>
        /// Takes the data of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public IList<ArraySegment<byte>> Take(int length)
        {
            var bufferList = m_Segments as BufferList;

            if (bufferList != null)
                return bufferList.Clone(m_CurrentSegmentIndex, m_CurrentSegmentOffset, length);

            return Clone(m_CurrentSegmentIndex, m_CurrentSegmentOffset, length);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Reset();
        }
    }
}

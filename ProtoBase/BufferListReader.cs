using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
        /// Gets or sets the logical position.
        /// </summary>
        /// <value>
        /// The logical position.
        /// </value>
        long Position { get; set; }

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
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
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
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
        /// <returns></returns>
        UInt16 ReadUInt16(bool littleEndian);

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <returns></returns>
        Int32 ReadInt32();

        /// <summary>
        /// Reads a Int32 number from the current data source.
        /// </summary>
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
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
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
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
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
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
        /// <param name="littleEndian">if set to <c>true</c> read the value as little endian, otherwise big endian.</param>
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
        /// <param name="length">The length of the string in bytes.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        string ReadString(int length, Encoding encoding);


        /// <summary>
        /// Skips the specified count bytes from the data source.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        IBufferReader Skip(int count);

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
        private const string c_ThreadBufferSegmentReader = "ThreadBufferListReader";

        /// <summary>
        /// The segments that make up the buffer.
        /// </summary>
        private IList<ArraySegment<byte>> m_Segments;

        /// <summary>
        /// The total length of all the segments.
        /// </summary>
        private long m_Length;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        /// <summary>
        /// Buffer used for temporary storage before conversion into primitives
        /// </summary>
        private byte[] m_Buffer = new byte[8];

        /// <summary>
        /// Gets the total length of the all bufers in bytes.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length
        {
            get { return m_Length; }
        }

        /// <summary>
        /// Gets or sets the logical position.
        /// </summary>
        /// <value>
        /// The logical position.
        /// </value>
        /// <exception cref="System.InvalidOperationException">
        /// Not initialized or trying to set the position outside the bounds of the buffer list.
        /// </exception>
        public long Position
        {
            get
            {
                CheckInitialized();

                long position = GetCurrentPosition();
                return position;
            }

            set
            {
                CheckInitialized();

                if (value < 0)
                {
                    throw new InvalidOperationException("Cannot position before the beginning of the buffer.");
                }

                if (Length < value)
                {
                    throw new InvalidOperationException("Cannot position past the end of the buffer.");
                }

                SetCurrentPosition(value);
            }
        }


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
            if (reader == null)
            {
                reader = new TReader();
                Thread.SetData(slot, reader);
            }

            return reader;
        }

        /// <summary>
        /// Initializes the reader with the data source segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <exception cref="System.ArgumentException">
        /// The number of elements containted in <paramref name="segments"/> must be greater than zero.
        /// </exception>
        public void Initialize(IList<ArraySegment<byte>> segments)
        {
            if (segments == null)
                throw new ArgumentNullException("segments");

            if (segments.Count == 0)
                throw new ArgumentException("The length of segments must be greater than zero.", "segments");

            m_Segments = segments;
            m_CurrentSegmentOffset = segments[0].Offset;

            long length = 0;

            for (var i = 0; i < segments.Count; i++)
            {
                length += segments[i].Count;
            }

            m_Length = length;
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
            {
                return unchecked((short)(BigEndianFromBytes(buffer, 2)));
            }

            return unchecked((short)(LittleEndianFromBytes(buffer, 2)));
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
            {
                return unchecked((ushort)(BigEndianFromBytes(buffer, 2)));
            }

            return unchecked((ushort)(LittleEndianFromBytes(buffer, 2)));
        }

        /// <summary>
        /// Skips the specified count bytes from the data source.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// count;count cannot be negative
        /// or
        /// count;exceed the total length
        /// </exception>
        public IBufferReader Skip(int count)
        {
            CheckInitialized();

            if (count == 0)
                return this;

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");

            if (Length < count)
            {
                throw new ArgumentOutOfRangeException("count", "Cannot be greater than the length of all the buffers.");
            }

            Position += count;

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
            {
                return unchecked((int)(BigEndianFromBytes(buffer, 4)));
            }

            return unchecked((int)(LittleEndianFromBytes(buffer, 4)));
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
            {
                return unchecked((uint)(BigEndianFromBytes(buffer, 4)));
            }

            return unchecked((uint)(LittleEndianFromBytes(buffer, 4)));
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
            FillBuffer(8);

            var buffer = m_Buffer;

            if (!littleEndian)
            {
                return unchecked(BigEndianFromBytes(buffer, 8));
            }

            return unchecked(LittleEndianFromBytes(buffer, 8));
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
            FillBuffer(8);

            var buffer = m_Buffer;

            if (!littleEndian)
            {
                return (ulong)BigEndianFromBytes(buffer, 8);
            }

            return (ulong)LittleEndianFromBytes(buffer, 8);
        }


        /// <summary>
        /// Reads a byte from the data source
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Reach the end of the data source</exception>
        public byte ReadByte()
        {
            if (!IsDataAvailable())
                throw new Exception("Reach the end of the data source");

            var currentSegment = m_Segments[m_CurrentSegmentIndex];

            var targetByte = currentSegment.Array[m_CurrentSegmentOffset];
            var maxOffset = currentSegment.Offset + currentSegment.Count - 1;
            var nextOffset = m_CurrentSegmentOffset + 1;

            if (nextOffset <= maxOffset)
            {
                // next pos is within the current segment
                m_CurrentSegmentOffset = nextOffset;
            }
            else
            {
                // next pos is within the next segment
                var nextSegmentIndex = m_CurrentSegmentIndex + 1;

                if (nextSegmentIndex < m_Segments.Count)
                {
                    m_CurrentSegmentIndex = nextSegmentIndex;
                    m_CurrentSegmentOffset = m_Segments[nextSegmentIndex].Offset;
                }
                else
                {
                    m_CurrentSegmentOffset = nextOffset; // End of Data
                    Debug.Assert(!IsDataAvailable());
                }
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
            if (output == null)
                throw new ArgumentNullException("output");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Non-negative number required.");

            if (output.Length - offset < count)
                throw new ArgumentException("Count cannot be less than zero.");

            return Read(output, offset, count);
        }

        /// <summary>
        /// Reads a string from the current data source
        /// </summary>
        /// <param name="length">The length of the string in bytes.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">length;there is no enougth data</exception>
        public string ReadString(int length, Encoding encoding)
        {
            var output = new char[encoding.GetMaxCharCount(length)];

            var decoder = encoding.GetDecoder();

            int currentSegmentIndex = m_CurrentSegmentIndex;
            int currentOffset = m_CurrentSegmentOffset;

            int charIndex = 0;

            while (currentSegmentIndex < m_Segments.Count)
            {
                int bytesUsed;
                int charsUsed;
                bool completed;

                var segment = m_Segments[currentSegmentIndex];
                if (currentSegmentIndex != m_CurrentSegmentIndex)
                    currentOffset = segment.Offset;

                if ((currentOffset + length) < (segment.Offset + segment.Count))
                {
                    // current segment has enough data
                    int byteCount = length;
                    int charCount = output.Length - charIndex;
                    decoder.Convert(segment.Array, currentOffset, byteCount, output, charIndex, charCount, true, out bytesUsed, out charsUsed, out completed);
                    charIndex += charsUsed;
                }
                else
                {
                    // consume the rest of the current segment
                    int byteCount = segment.Count - (currentOffset - segment.Offset);
                    bool flush = byteCount == length;
                    int charCount = output.Length - charIndex;
                    decoder.Convert(segment.Array, currentOffset, byteCount, output, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
                    charIndex += charsUsed;
                }

                length -= bytesUsed;
                currentOffset += bytesUsed;

                if (length == 0)
                {
                    break;
                }

                if (currentSegmentIndex != m_Segments.Count - 1)
                {
                    currentSegmentIndex++;
                }
            }

            m_CurrentSegmentIndex = currentSegmentIndex;
            m_CurrentSegmentOffset = currentOffset;

            return new string(output, 0, charIndex);
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
            CheckInitialized();

            if (length > 8)
                throw new ArgumentOutOfRangeException("length", "the length must between 1 and 8");

            var read = Read(m_Buffer, 0, length);

            if (read != length)
                throw new ArgumentOutOfRangeException("length", "there is no enough data to read");
        }

        /// <summary>
        /// Check to see if this instance is initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not initialized.s
        /// </exception>
        private void CheckInitialized()
        {
            if (m_Segments == null)
            {
                throw new InvalidOperationException("Not initialized");
            }
        }

        private int Read(byte[] buffer, int offset, int count)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(0 <= offset);
            Debug.Assert(0 < count);
            Debug.Assert(0 <= offset);
            Debug.Assert(buffer.Length - offset >= count);

            if (!IsDataAvailable())
                throw new Exception("Reach the end of the data source");

            var currentOffset = m_CurrentSegmentOffset;
            var len = 0;
            var segIndex = 0;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];
                if (i != m_CurrentSegmentIndex)
                    currentOffset = segment.Offset;

                if ((currentOffset + count) <= (segment.Offset + segment.Count))
                {

                }

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

                    return len;
                }
            }

            //No enougth data to read
            m_CurrentSegmentIndex = segIndex;
            m_CurrentSegmentOffset = 0;

            return len;
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
        /// Determines if there is any more data available base on the current position.
        /// </summary>
        /// <returns></returns>
        private bool IsDataAvailable()
        {
            if (m_Segments == null)
                return false;

            var currentSegment = m_Segments[m_CurrentSegmentIndex];

            if ((m_CurrentSegmentIndex + 1) == m_Segments.Count && m_CurrentSegmentOffset == currentSegment.Count + currentSegment.Offset)
            {
                return false;
            }

            return true;
        }

        private long GetCurrentPosition()
        {
            long position = 0;
            for (int i = 0; i < m_CurrentSegmentIndex; i++)
            {
                position += m_Segments[i].Count;
            }

            position += m_CurrentSegmentOffset - m_Segments[m_CurrentSegmentIndex].Offset;

            return position;
        }

        private void SetCurrentPosition(long position)
        {
            long currentPosition = GetCurrentPosition();
            long delta = position - currentPosition;

            if (delta == 0)
            {
                return;
            }

            var segment = m_Segments[m_CurrentSegmentIndex];

            if (0 < delta)
            {
                // moving forward
                if (delta <= BytesRemaining(segment, m_CurrentSegmentOffset))
                {
                    // move forward in the same segment
                    m_CurrentSegmentOffset += (int)delta;
                }
                else
                {
                    delta -= BytesRemaining(segment, m_CurrentSegmentOffset);

                    for (int i = m_CurrentSegmentIndex + 1; i < m_Segments.Count; i++)
                    {
                        segment = m_Segments[i];
                        if (delta < segment.Count)
                        {
                            m_CurrentSegmentIndex = i;
                            m_CurrentSegmentOffset = (int)(segment.Offset + delta);
                            break;
                        }

                        delta -= segment.Count;
                    }
                }
            }
            else
            {
                // moving backward (note: delta is negative)
                if (0 <= (m_CurrentSegmentOffset + delta - segment.Offset))
                {
                    // move back in the same segment
                    m_CurrentSegmentOffset += (int)delta;
                }
                else
                {
                    delta += m_CurrentSegmentOffset - segment.Offset;

                    for (int i = m_CurrentSegmentIndex - 1; 0 <= i; i--)
                    {
                        segment = m_Segments[i];
                        if (0 <= delta + segment.Count)
                        {
                            m_CurrentSegmentIndex = i;
                            m_CurrentSegmentOffset = (int)(segment.Count + segment.Offset + delta);
                            break;
                        }

                        delta += segment.Count;
                    }
                 }                
            }
        }

        private static int BytesRemaining(ArraySegment<byte> segment, int currentOffset)
        {
            return segment.Count - currentOffset + segment.Offset;
        }

        /// <summary>
        /// Gets a <see cref="long"/> value by reading the buffer as a big endian integer.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="bytesToConvert">The number bytes to convert.</param>
        /// <returns></returns>
        /// <remarks>
        /// Code taken from Jon Skeet's Miscellaneous Utility Library
        /// &lt;a href="http://www.yoda.arachsys.com/csharp/miscutil/"&gt;Jon Skeet's Miscellaneous Utility Library&lt;/a&gt;
        /// </remarks>
        private long BigEndianFromBytes(byte[] buffer, int bytesToConvert)
        {
            long ret = 0;
            for (int i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | buffer[i]);
            }
            return ret;
        }

        /// <summary>
        /// Gets a <see cref="long"/> value by reading the buffer as a little endian integer.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="bytesToConvert">The number bytes to convert.</param>
        /// <returns></returns>
        /// <remarks>
        /// Code taken from Jon Skeet's Miscellaneous Utility Library
        /// &lt;a href="http://www.yoda.arachsys.com/csharp/miscutil/"&gt;Jon Skeet's Miscellaneous Utility Library&lt;/a&gt;
        /// </remarks>
        private long LittleEndianFromBytes(byte[] buffer, int bytesToConvert)
        {
            long ret = 0;
            for (int i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | buffer[bytesToConvert - 1 - i]);
            }
            return ret;
        }
    }
}

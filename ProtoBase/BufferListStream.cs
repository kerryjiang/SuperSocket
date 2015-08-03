using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for the stream class whose data is consistent of many data segments
    /// </summary>
    public interface IBufferListStream
    {
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
        /// Takes the data of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        IList<ArraySegment<byte>> Take(int length);

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
        IBufferListStream Skip(int count);

        /// <summary>
        /// Get current buffer as Stream
        /// </summary>
        /// <returns>the stream represent the current buffer</returns>
        Stream GetCurrentStream();
    }

    /// <summary>
    /// The default buffer list stream
    /// </summary>
    public class BufferListStream : Stream, IBufferListStream
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

        /// <summary>
        /// Buffer used for temporary storage before conversion into primitives
        /// </summary>
        private byte[] m_Buffer = new byte[8];

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferListStream"/> class.
        /// </summary>
        public BufferListStream()
        {
            
        }

        /// <summary>
        /// Initializes the specified segments.
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


#if !PCL
        private const string c_ThreadBufferSegmentStream = "ThreadBufferListStream";

        /// <summary>
        /// Gets the current buffer segment stream from the thread context
        /// </summary>
        /// <value>
        /// The current buffer segment stream
        /// </value>
        public static BufferListStream GetCurrent()
        {
            return GetCurrent<BufferListStream>();
        }

        /// <summary>
        /// Gets the current buffer segment stream from the thread context
        /// </summary>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <returns></returns>
        public static TStream GetCurrent<TStream>()
            where TStream : BufferListStream, new()
        {
            var slot = Thread.GetNamedDataSlot(c_ThreadBufferSegmentStream);
            var stream = Thread.GetData(slot) as TStream;
            if (stream != null)
                return stream;

            stream = new TStream();
            Thread.SetData(slot, stream);
            return stream;
        }
#endif

        /// <summary>
        /// Get current buffer as Stream
        /// </summary>
        /// <returns>the stream represent the current buffer</returns>
        public Stream GetCurrentStream()
        {
            return this;
        }

        /// <summary>
        /// Resets this stream.
        /// </summary>
        public void Reset()
        {
            m_Segments = null;
            m_CurrentSegmentIndex = 0;
            m_CurrentSegmentOffset = 0;
            m_Length = 0;
            m_Position = 0;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return m_Position < m_Length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override long Length
        {
            get { return m_Length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var currentOffset = m_CurrentSegmentOffset;
            var len = 0;
            var segIndex = 0;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];

                if(i != m_CurrentSegmentIndex)
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
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.ArgumentException">Cannot support seek from the end.</exception>
        /// <exception cref="System.Exception">Exceed the stream's end</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
                throw new ArgumentException("Cannot support seek from the end.");

            if (origin == SeekOrigin.Begin)
            {
                m_CurrentSegmentIndex = 0;
                m_CurrentSegmentOffset = m_Segments[0].Offset;
                m_Position = 0;
            }

            if (offset == 0)
                return m_Position;

            long startPos = origin == SeekOrigin.Begin ? 0 : m_Position;
            long targetPos = startPos + offset;
            long segMaxOffset = m_Position;

            for (var i = m_CurrentSegmentIndex; i < m_Segments.Count; i++)
            {
                var segment = m_Segments[i];
                var currentOffset = 0;

                if (i == m_CurrentSegmentIndex)
                {
                    currentOffset = m_CurrentSegmentOffset;
                    segMaxOffset += segment.Count - (m_CurrentSegmentOffset - segment.Offset);
                }
                else
                {
                    currentOffset = segment.Offset;
                    segMaxOffset += segment.Count;
                }

                if (segMaxOffset < targetPos)
                    continue;

                var subPos = (int)(segMaxOffset - targetPos);

                m_CurrentSegmentIndex = i;
                m_CurrentSegmentOffset = segment.Offset + segment.Count - subPos;
                m_Position = targetPos;
                return targetPos;
            }

            throw new Exception("Exceed the stream's end");
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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

        /// <summary>
        /// Skips the specified count bytes from the data source.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// count;count cannot be negative
        /// or
        /// count;exceed the total length
        /// </exception>
        public IBufferListStream Skip(int count)
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
        /// Reads a string from the current data source
        /// </summary>
        /// <param name="length">The length of the string in bytes.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">length;there is no enougth data</exception>
        public string ReadString(int length, Encoding encoding)
        {
            var readLength = length;
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
            m_Position += readLength;

            return new string(output, 0, charIndex);
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

            if (read != length)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The default buffer list stream
    /// </summary>
    public class BufferListStream : Stream
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

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
                m_Position = value;
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
    }
}

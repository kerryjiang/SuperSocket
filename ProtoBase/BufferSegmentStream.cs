using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.ProtoBase
{
    public sealed class BufferSegmentStream : Stream
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

        public BufferSegmentStream()
        {
            
        }

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

        public void Reset()
        {
            m_Segments = null;
            m_CurrentSegmentIndex = 0;
            m_CurrentSegmentOffset = 0;
            m_Length = 0;
            m_Position = 0;
        }

        public override bool CanRead
        {
            get { return m_Position < m_Length; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return m_Length; }
        }

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

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

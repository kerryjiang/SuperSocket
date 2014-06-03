using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    public interface IBufferReader
    {
        void Initialize(IList<ArraySegment<byte>> segments);

        void Reset();

        Int16 ReadInt16();

        Int16 ReadInt16(bool littleEndian);

        UInt16 ReadUInt16();

        UInt16 ReadUInt16(bool littleEndian);

        void Skip(int count);

        Int32 ReadInt32();

        Int32 ReadInt32(bool littleEndian);

        UInt32 ReadUInt32();

        UInt32 ReadUInt32(bool littleEndian);

        Int64 ReadInt64();

        Int64 ReadInt64(bool littleEndian);

        UInt64 ReadUInt64();

        UInt64 ReadUInt64(bool littleEndian);

        byte ReadByte();

        int ReadBytes(byte[] output, int offset, int count);

        string ReadString(int length, Encoding encoding);
    }

    public class BufferSegmentReader : IBufferReader
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

        private byte[] m_Buffer = new byte[8];

        public BufferSegmentReader()
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

        private const string c_ThreadBufferSegmentReader = "ThreadBufferSegmentReader";

        public static IBufferReader GetCurrent()
        {
            return GetCurrent<BufferSegmentReader>();
        }

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

        protected void FillBuffer(int length)
        {
            if (length > 8)
                throw new ArgumentOutOfRangeException("length", "the length must between 1 and 8");

            var read = Read(m_Buffer, 0, length);

            if(read != length)
                throw new ArgumentOutOfRangeException("length", "there is no enough data to read");
        }

        public short ReadInt16()
        {
            return ReadInt16(false);
        }

        public short ReadInt16(bool littleEndian)
        {
            FillBuffer(2);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (short)((int)buffer[0] | (int)buffer[1] << 8);
            else
                return (short)((int)buffer[1] | (int)buffer[0] << 8);
        }

        public ushort ReadUInt16()
        {
            return ReadUInt16(false);
        }

        public ushort ReadUInt16(bool littleEndian)
        {
            FillBuffer(2);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (ushort)((int)buffer[0] | (int)buffer[1] << 8);
            else
                return (ushort)((int)buffer[1] | (int)buffer[0] << 8);
        }

        private void SyncCurrentSegment()
        {
            //TODO: reset current segment
        }

        public void Skip(int count)
        {
            if(count <= 0)
                throw new ArgumentOutOfRangeException("count", "count cannot be zero or negative");

            var pos = m_Position + count;
            
            if(pos >= m_Length)
                throw new ArgumentOutOfRangeException("count", "exceed the total length");

            m_Position = pos;
            SyncCurrentSegment();
        }

        public int ReadInt32()
        {
            return ReadInt32(false);
        }

        public int ReadInt32(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (int)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
            else
                return (int)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
        }

        public uint ReadUInt32()
        {
            return ReadUInt32(false);
        }

        public uint ReadUInt32(bool littleEndian)
        {
            FillBuffer(4);

            var buffer = m_Buffer;

            if (!littleEndian)
                return (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
            else
                return (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
        }

        public long ReadInt64()
        {
            return ReadInt64(false);
        }

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

        public ulong ReadUInt64()
        {
            throw new NotImplementedException();
        }

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

        public byte ReadByte()
        {
            throw new NotSupportedException();
        }

        public int ReadBytes(byte[] output, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public string ReadString(int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}

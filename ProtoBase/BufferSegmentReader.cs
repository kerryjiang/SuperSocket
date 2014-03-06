using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.ProtoBase
{
    public interface IBufferReader
    {
        void Initialize(IList<ArraySegment<byte>> segments);

        void Reset();

        void ReadInt16();

        void ReadInt16(bool littleEndian);

        void ReadUInt16();

        void ReadUInt16(bool littleEndian);

        void Skip(int count);

        void ReadInt32();

        void ReadInt32(bool littleEndian);

        void ReadUInt32();

        void ReadUInt32(bool littleEndian);

        void ReadInt64();

        void ReadInt64(bool littleEndian);

        void ReadUInt64();

        void ReadUInt64(bool littleEndian);

        byte ReadByte();

        int ReadBytes(byte[] output, int offset, int count);

        byte ReadString(int length, Encoding encoding);
    }

    public class BufferSegmentReader : IBufferReader
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private int m_CurrentSegmentIndex;

        private int m_CurrentSegmentOffset;

        private long m_Length;

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

        public void Reset()
        {
            m_Segments = null;
            m_CurrentSegmentIndex = 0;
            m_CurrentSegmentOffset = 0;
            m_Length = 0;
            m_Position = 0;
        }


        public void ReadInt16()
        {
            throw new NotImplementedException();
        }

        public void ReadInt16(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public void ReadUInt16()
        {
            throw new NotImplementedException();
        }

        public void ReadUInt16(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public void Skip(int count)
        {
            throw new NotImplementedException();
        }

        public void ReadInt32()
        {
            throw new NotImplementedException();
        }

        public void ReadInt32(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public void ReadUInt32()
        {
            throw new NotImplementedException();
        }

        public void ReadUInt32(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public void ReadInt64()
        {
            throw new NotImplementedException();
        }

        public void ReadInt64(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public void ReadUInt64()
        {
            throw new NotImplementedException();
        }

        public void ReadUInt64(bool littleEndian)
        {
            throw new NotImplementedException();
        }

        public byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public int ReadBytes(byte[] output, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public byte ReadString(int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}

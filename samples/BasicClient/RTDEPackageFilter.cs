using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ProtoBase;

namespace BasicClient
{
    public class RTDEPackageFilter : FixedHeaderPipelineFilter<RTDEPackage>
    {
        /// <summary>
        /// Header size is 3
        /// 00-01 data frame length
        /// 02 RoboCmd
        /// </summary>
        public RTDEPackageFilter() : base(3)
        {
        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.TryReadBigEndian(out short len);

            return len - 2;
        }

        protected override RTDEPackage DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var package = new RTDEPackage();

            var reader = new SequenceReader<byte>(buffer);

            reader.TryRead(out byte cmd);
            package.RoboCmd = (RoboCmd)cmd;

            package.Data = reader.ReadString(Encoding.UTF8);
            return package;
        }
    }
    public class RTDEPackageEncoder : IPackageEncoder<RTDEPackage>
    {
        public int Encode(IBufferWriter<byte> writer, RTDEPackage pack)
        {
            var length = pack.Data.Length + 3;
            byte[] bytes = new byte[length];
            bytes[0] = (byte)(length / 256);
            bytes[1] = (byte)(length % 256);
            bytes[2] = (byte)pack.RoboCmd;
            var dataBytes = Encoding.UTF8.GetBytes(pack.Data);
            Array.Copy(dataBytes, 0, bytes, 3, pack.Data.Length);

            var span = new ReadOnlySpan<byte>(bytes, 0, bytes.Length);

            writer.Write(span);
            return length;
        }
    }
}

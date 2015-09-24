using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Encoders
{
    class Rfc6455ProtoDataEncoder : IProtoDataEncoder
    {
        protected ArraySegment<byte> EncodeData(OpCode opCode, bool isFinal, ArraySegment<byte> data)
        {
            var length = data.Count;

            byte[] fragment;

            if (length < 126)
            {
                fragment = new byte[2 + length];
                fragment[1] = (byte)length;
            }
            else if (length < 65536)
            {
                fragment = new byte[4 + length];
                fragment[1] = (byte)126;
                fragment[2] = (byte)(length / 256);
                fragment[3] = (byte)(length % 256);
            }
            else
            {
                fragment = new byte[10 + length];
                fragment[1] = 127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    fragment[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }

            var intOpCode = (int)opCode;

            if (isFinal)
                fragment[0] = (byte)(intOpCode | 0x80); //1000 0000
            else
                fragment[0] = (byte)intOpCode;

            if (length > 0)
            {
                Buffer.BlockCopy(data.Array, data.Offset, fragment, fragment.Length - length, length);
            }

            return new ArraySegment<byte>(fragment);
        }

        public IList<ArraySegment<byte>> EncodeData(IList<ArraySegment<byte>> data)
        {
            var result = new List<ArraySegment<byte>>(data.Count);
            var lastIndex = data.Count - 1;

            for(var i = 0; i < data.Count; i++)
            {
                result.Add(EncodeData(i == 0 ? OpCode.Binary : OpCode.Continuation, i == lastIndex, data[i]));
            }

            return result;
        }

        public IList<ArraySegment<byte>> EncodeData(ArraySegment<byte> data)
        {
            return new ArraySegment<byte>[] { EncodeData(OpCode.Binary, true, data) };
        }
    }
}

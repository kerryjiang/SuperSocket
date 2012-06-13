using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyRequestFilter : RequestFilterBase<BinaryRequestInfo>
    {
        private readonly MyDataRequestFilter m_PreparedDataReader = new MyDataRequestFilter();

        private MyDataRequestFilter GetMyCommandDataReader(string commandName, int dataLength)
        {
            m_PreparedDataReader.Initialize(commandName, dataLength, this);
            return m_PreparedDataReader;
        }

        public override BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

            int leftLength = 6 - this.BufferSegments.Count;

            if (length < leftLength)
            {
                AddArraySegment(readBuffer, offset, length, toBeCopied);
                NextRequestFilter = this;
                return null;
            }

            AddArraySegment(readBuffer, offset, leftLength, toBeCopied);

            string commandName = BufferSegments.Decode(Encoding.ASCII, 0, 4);

            int commandDataLength = (int)BufferSegments[4] * 256 + (int)BufferSegments[5];

            if (length > leftLength)
            {
                int leftDataLength = length - leftLength;
                if (leftDataLength >= commandDataLength)
                {
                    byte[] commandData = readBuffer.CloneRange(offset + leftLength, commandDataLength);
                    BufferSegments.ClearSegements();
                    NextRequestFilter = this;
                    var requestInfo = new BinaryRequestInfo(commandName, commandData);

                    //The next requestInfo is comming
                    if (leftDataLength > commandDataLength)
                        left = leftDataLength - commandDataLength;

                    return requestInfo;
                }
                else// if (leftDataLength < commandDataLength)
                {
                    //Clear previous cached header data
                    BufferSegments.ClearSegements();
                    //Save left data part
                    AddArraySegment(readBuffer, offset + leftLength, length - leftLength, toBeCopied);
                    NextRequestFilter = GetMyCommandDataReader(commandName, commandDataLength);
                    return null;
                }
            }
            else
            {
                NextRequestFilter = GetMyCommandDataReader(commandName, commandDataLength);
                return null;
            }
        }
    }
}

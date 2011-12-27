using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.Common;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyCommandReader : CommandReaderBase<BinaryCommandInfo>
    {
        private readonly MyCommandDataReader m_PreparedDataReader = new MyCommandDataReader();

        public MyCommandReader(IAppServer appServer)
            : base(appServer)
        {

        }

        private MyCommandDataReader GetMyCommandDataReader(string commandName, int dataLength)
        {
            m_PreparedDataReader.Initialize(commandName, dataLength, this);
            return m_PreparedDataReader;
        }

        /// <summary>
        /// Finds the command.
        /// "SEND 0008 xg^89W(v"
        /// Read 10 chars as command name and command data length
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReusableBuffer">if set to <c>true</c> [is reusable buffer].</param>
        /// <returns></returns>
        public override BinaryCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            left = 0;

            int leftLength = 10 - this.BufferSegments.Count;

            if (length < leftLength)
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                NextCommandReader = this;
                return null;
            }

            AddArraySegment(readBuffer, offset, leftLength, isReusableBuffer);

            string commandName = BufferSegments.Decode(Encoding.ASCII, 0, 4);
            int commandDataLength = Convert.ToInt32(BufferSegments.Decode(Encoding.ASCII, 5, 4).TrimStart('0'));

            if (length > leftLength)
            {
                int leftDataLength = length - leftLength;
                if (leftDataLength >= commandDataLength)
                {
                    byte[] commandData = readBuffer.CloneRange(offset + leftLength, commandDataLength);
                    BufferSegments.ClearSegements();
                    NextCommandReader = this;
                    var commandInfo = new BinaryCommandInfo(commandName, commandData);

                    //The next commandInfo is comming
                    if (leftDataLength > commandDataLength)
                        left = leftDataLength - commandDataLength;

                    return commandInfo;
                }
                else// if (leftDataLength < commandDataLength)
                {
                    //Clear previous cached header data
                    BufferSegments.ClearSegements();
                    //Save left data part
                    AddArraySegment(readBuffer, offset + leftLength, length - leftLength, isReusableBuffer);
                    NextCommandReader = GetMyCommandDataReader(commandName, commandDataLength);
                    return null;
                }
            }
            else
            {
                NextCommandReader = GetMyCommandDataReader(commandName, commandDataLength);
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;
using SuperSocket.Common;

namespace SuperSocket.QuickStart.SocksServer
{
    public class DataCommandReader : CommandReaderBase<BinaryCommandInfo>
    {
        public DataCommandReader(ICommandReader<BinaryCommandInfo> previousCommandReader)
            : base(previousCommandReader.AppServer)
        {

        }

        public override BinaryCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            this.NextCommandReader = this;
            return new BinaryCommandInfo(SocksConst.DATA, readBuffer.CloneRange(offset, length));
        }
    }
}

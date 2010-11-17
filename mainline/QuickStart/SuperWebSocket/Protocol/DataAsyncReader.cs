using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Protocol;
using SuperSocket.Common;

namespace SuperWebSocket.Protocol
{
    public class DataAsyncReader : AsyncReaderBase
    {
        public DataAsyncReader(HeaderAsyncReader headerReader)
        {
            Segments = headerReader.GetLeftBuffer();
        }

        #region ICommandAsyncReader Members

        public override bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

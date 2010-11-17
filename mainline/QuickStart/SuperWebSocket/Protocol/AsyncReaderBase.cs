using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Protocol;
using SuperSocket.Common;

namespace SuperWebSocket.Protocol
{
    public abstract class AsyncReaderBase : ICommandAsyncReader
    {
        protected ArraySegmentList<byte> Segments { get; set; }

        #region ICommandAsyncReader Members

        public abstract bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return Segments;
        }

        public ICommandAsyncReader NextCommandReader { get; protected set; }

        #endregion
    }
}

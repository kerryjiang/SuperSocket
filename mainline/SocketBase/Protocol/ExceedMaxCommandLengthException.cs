using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public class ExceedMaxCommandLengthException : Exception
    {
        public int MaxCommandLength { get; private set; }
        public int CurrentProcessedLength { get; private set; }

        public ExceedMaxCommandLengthException(int maxCommandLength, int currentProcessedLength)
        {
            MaxCommandLength = maxCommandLength;
            CurrentProcessedLength = currentProcessedLength;
        }
    }
}

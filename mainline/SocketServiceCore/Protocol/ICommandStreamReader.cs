using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public interface ICommandStreamReader
    {
        void InitializeReader(Stream stream, Encoding encoding, int bufferSize);

        string ReadCommand();
    }
}

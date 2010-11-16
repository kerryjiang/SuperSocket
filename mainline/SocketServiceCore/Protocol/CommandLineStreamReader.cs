using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public class CommandLineStreamReader : ICommandStreamReader
    {
        private StreamReader m_CmdLineReader;

        public CommandLineStreamReader()
        {
            
        }

        #region ICommandStreamReader Members

        public void InitializeReader(Stream stream, Encoding encoding, int bufferSize)
        {
            m_CmdLineReader = new StreamReader(stream, encoding, false, bufferSize);
        }

        public string ReadCommand()
        {
            return m_CmdLineReader.ReadLine();
        }       

        #endregion
    }
}

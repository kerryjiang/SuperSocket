using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public class CommandLineProtocol : ISyncProtocol, IAsyncProtocol
    {
        #region IAsyncProtocol Members

        public ICommandAsyncReader CreateAsyncCommandReader()
        {
            return new NewTerminatorCommandAsyncReader(Encoding.UTF8.GetBytes(Environment.NewLine));
        }

        #endregion

        #region ISyncProtocol Members

        public ICommandStreamReader CreateSyncCommandReader()
        {
            return new TerminatorCommandStreamReader(Environment.NewLine);
            //return new CommandLineStreamReader();
        }

        #endregion
    }
}

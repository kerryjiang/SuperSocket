using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public class NewTerminatorCommandAsyncReader : TerminatorCommandAsyncReaderBase
    {
        public NewTerminatorCommandAsyncReader(TerminatorCommandAsyncReaderBase prevCommandReader)
            : base(prevCommandReader)
        {

        }

        public NewTerminatorCommandAsyncReader(byte[] token)
            : base(token)
        {

        }

        public override bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData)
        {
            var result = FindCommandDirectly(readBuffer, offset, length, out commandData);
            NextCommandReader = CreateNextCommandReader(result);
            return result.Status == SearhTokenStatus.Found;
        }
    }
}

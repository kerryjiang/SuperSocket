using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession
    {
        void Connect();

        void Send(byte[] data, int offset, int length);

        void Close();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.ClientEngine
{
    public static class SocketEx
    {
        public static void SendData(this Socket client, byte[] data)
        {
            SendData(client, data, 0, data.Length);
        }

        public static void SendData(this Socket client, byte[] data, int offset, int length)
        {
            int sent = 0;
            int thisSent = 0;

            while ((length - sent) > 0)
            {
                thisSent = client.Send(data, offset + sent, length - sent, SocketFlags.None);
                sent += thisSent;
            }
        }
    }
}

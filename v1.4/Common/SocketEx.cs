using System;
using System.Net.Sockets;

namespace SuperSocket.Common
{
    public static class SocketEx
    {
        public static void SafeCloseClientSocket(this Socket client, ILogger logger)
        {
            if(client == null)
                return;

            if (!client.Connected)
                return;
            
            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch(ObjectDisposedException)
            {
            }
            catch(Exception e)
            {
                if(logger != null)
                    logger.LogError(e);
            }
            
            try
            {
                client.Close();
            }
            catch(ObjectDisposedException)
            {
            }
            catch(Exception e)
            {
                if(logger != null)
                    logger.LogError(e);
            }
        }
        
        public static void SafeCloseClientSocket(this Socket client)
        {
            //No logger
            client.SafeCloseClientSocket(null);
        }

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


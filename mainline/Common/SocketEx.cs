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
    }
}


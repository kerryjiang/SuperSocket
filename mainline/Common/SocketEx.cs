using System;
using System.Net.Sockets;
using SuperSocket.Common.Logging;

namespace SuperSocket.Common
{
    /// <summary>
    /// Socket extension class
    /// </summary>
    public static class SocketEx
    {
        /// <summary>
        /// Safes the close client socket.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="logger">The logger.</param>
        public static void SafeCloseClientSocket(this Socket client, ILog logger)
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
                    logger.Error(e);
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
                    logger.Error(e);
            }
        }

        /// <summary>
        /// Safes the close client socket.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void SafeCloseClientSocket(this Socket client)
        {
            //No logger
            client.SafeCloseClientSocket(null);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="data">The data.</param>
        public static void SendData(this Socket client, byte[] data)
        {
            SendData(client, data, 0, data.Length);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
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


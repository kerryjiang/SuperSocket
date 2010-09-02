using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    interface IAsyncCommandReader
    {
        SearhMarkResult FindCommand(SocketAsyncEventArgs e, byte[] endMark, out byte[] commandData);

        List<Byte> GetLeftBuffer();
    }

    abstract class AsyncCommandReader : IAsyncCommandReader
    {
        private List<Byte> receiveBuffer;

        public AsyncCommandReader()
        {
            receiveBuffer = new List<byte>();
        }

        public AsyncCommandReader(IAsyncCommandReader prevReader)
        {
            receiveBuffer = prevReader.GetLeftBuffer();
        }

        #region IAsyncCommandReader Members

        public abstract SearhMarkResult FindCommand(SocketAsyncEventArgs e, byte[] endMark, out byte[] commandData);

        public List<byte> GetLeftBuffer()
        {
            return receiveBuffer;
        }

        #endregion

        protected List<Byte> SaveBuffer(IList<byte> newData)
        {
            receiveBuffer.AddRange(newData);
            return receiveBuffer;
        }

        protected List<Byte> SaveBuffer(IList<byte> newData, int offset, int length)
        {
            receiveBuffer.AddRange(newData.Skip(offset).Take(length));
            return receiveBuffer;
        }

        protected SearhMarkResult FindCommandDirectly(SocketAsyncEventArgs e, int offset, byte[] endMark, out byte[] commandData)
        {
            int? result = e.Buffer.SearchMark(offset, e.BytesTransferred, endMark);

            if (!result.HasValue)
            {
                SaveBuffer(e.Buffer, e.Offset, e.BytesTransferred);
                commandData = new byte[0];
                return new SearhMarkResult
                {
                    Status = SearhMarkStatus.None
                };
            }

            //Found endmark
            if (result.Value > 0)
            {
                var buffer = SaveBuffer(e.Buffer, e.Offset, result.Value - e.Offset);
                commandData = buffer.ToArray();
                buffer.Clear();

                SaveBuffer(e.Buffer, result.Value + endMark.Length, e.BytesTransferred - result.Value - endMark.Length);

                return new SearhMarkResult
                {
                    Status = SearhMarkStatus.Found,
                    Value = result.Value
                };
            }
            else
            {
                SaveBuffer(e.Buffer, e.Offset, e.BytesTransferred);
                commandData = new byte[0];
                return new SearhMarkResult
                {
                    Status = SearhMarkStatus.FoundStart,
                    Value = 0 - result.Value
                };
            }
        }
    }
}

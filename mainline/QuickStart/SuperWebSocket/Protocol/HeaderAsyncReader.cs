using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Protocol;
using SuperSocket.Common;

namespace SuperWebSocket.Protocol
{
    public class HeaderAsyncReader : AsyncReaderBase
    {
        private static readonly byte[] m_HeaderTerminator = Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine);

        public HeaderAsyncReader(HeaderAsyncReader prevHeaderReader)
        {
            Segments = prevHeaderReader.GetLeftBuffer();
        }

        public HeaderAsyncReader()
        {
            Segments = new ArraySegmentList<byte>();
        }

        #region ICommandAsyncReader Members

        public override bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData)
        {
            commandData = null;

            Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset, length));
            int? result = Segments.SearchMark(offset, length, m_HeaderTerminator);

            if (result.HasValue && result.Value > 0)
            {
                commandData = Segments.ToArrayData(0, result.Value + m_HeaderTerminator.Length);

                int left = Segments.Count - result.Value - m_HeaderTerminator.Length;

                Segments.ClearSegements();

                if (left > 0)
                {
                    Segments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));
                }

                NextCommandReader = new DataAsyncReader(this);
                return true;
            }
            else
            {
                NextCommandReader = new HeaderAsyncReader(this);
                return false;
            }
        }

        #endregion
    }
}

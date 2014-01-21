using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Protocol.FramePartReader;

namespace SuperSocket.WebSocket.Protocol
{
    class WebSocketDataFrameReceiveFilter : IReceiveFilter<IWebSocketFragment>
    {
        private WebSocketDataFrame m_Frame;
        private IDataFramePartReader m_PartReader;
        private int m_LastPartLength = 0;

        public int LeftBufferSize
        {
            get
            {
                if (m_Frame == null)
                    return 0;

                return m_Frame.InnerData.Count;
            }
        }

        public IReceiveFilter<IWebSocketFragment> NextReceiveFilter
        {
            get { return this; }
        }

        public WebSocketDataFrameReceiveFilter()
        {
            m_Frame = new WebSocketDataFrame(new ArraySegmentList());
            m_PartReader = DataFramePartReader.NewReader;
        }

        protected void AddArraySegment(ArraySegmentList segments, byte[] buffer, int offset, int length, bool toBeCopied)
        {
            segments.AddSegment(buffer, offset, length, toBeCopied);
        }

        public IWebSocketFragment Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            if (m_Frame == null)
                m_Frame = new WebSocketDataFrame(new ArraySegmentList());

            this.AddArraySegment(m_Frame.InnerData, readBuffer, offset, length, toBeCopied);

            IDataFramePartReader nextPartReader;

            int thisLength = m_PartReader.Process(m_LastPartLength, m_Frame, out nextPartReader);

            if (thisLength < 0)
            {
                left = 0;
                return null;
            }
            else
            {
                left = thisLength;

                if (left > 0)
                    m_Frame.InnerData.TrimEnd(left);

                //Means this part reader is the last one
                if (nextPartReader == null)
                {
                    m_LastPartLength = 0;
                    m_PartReader = DataFramePartReader.NewReader;

                    var frame = m_Frame;
                    m_Frame = null;
                    return frame;
                }
                else
                {
                    m_LastPartLength = m_Frame.InnerData.Count - thisLength;
                    m_PartReader = nextPartReader;

                    return null;
                }
            }
        }

        public void Reset()
        {
            m_Frame = null;
        }


        public FilterState State { get; private set; }
    }
}

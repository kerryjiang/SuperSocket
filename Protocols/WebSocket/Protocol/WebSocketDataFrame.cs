using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.WebSocket.Protocol
{
    class WebSocketDataFrame : IWebSocketFragment
    {
        private ArraySegmentList m_InnerData;

        public ArraySegmentList InnerData
        {
            get { return m_InnerData; }
        }

        public WebSocketDataFrame(ArraySegmentList data)
        {
            m_InnerData = data;
            m_InnerData.ClearSegements();
        }

        public bool FIN
        {
            get { return ((m_InnerData[0] & 0x80) == 0x80); }
        }

        public bool RSV1
        {
            get { return ((m_InnerData[0] & 0x40) == 0x40); }
        }

        public bool RSV2
        {
            get { return ((m_InnerData[0] & 0x20) == 0x20); }
        }

        public bool RSV3
        {
            get { return ((m_InnerData[0] & 0x10) == 0x10); }
        }

        public sbyte OpCode
        {
            get { return (sbyte)(m_InnerData[0] & 0x0f); }
        }

        public bool HasMask
        {
            get { return ((m_InnerData[1] & 0x80) == 0x80); }
        }

        public sbyte PayloadLenght
        {
            get { return (sbyte)(m_InnerData[1] & 0x7f); }
        }

        private long m_ActualPayloadLength = -1;

        public long ActualPayloadLength
        {
            get
            {
                if (m_ActualPayloadLength >= 0)
                    return m_ActualPayloadLength;

                var payloadLength = PayloadLenght;

                if (payloadLength < 126)
                    m_ActualPayloadLength = payloadLength;
                else if (payloadLength == 126)
                {
                    m_ActualPayloadLength = (int)m_InnerData[2] * 256 + (int)m_InnerData[3];
                }
                else
                {
                    long len = 0;
                    int n = 1;

                    for (int i = 7; i >= 0; i--)
                    {
                        len += (int)m_InnerData[i + 2] * n;
                        n *= 256;
                    }

                    m_ActualPayloadLength = len;
                }

                return m_ActualPayloadLength;
            }
        }

        public byte[] MaskKey { get; set; }

        public byte[] ExtensionData { get; set; }

        public byte[] ApplicationData { get; set; }

        public int Length
        {
            get { return m_InnerData.Count; }
        }

        public void Clear()
        {
            m_InnerData.ClearSegements();
            ExtensionData = new byte[0];
            ApplicationData = new byte[0];
            m_ActualPayloadLength = -1;
        }

        public string Key
        {
            get { return OpCode.ToString(); }
        }
    }
}

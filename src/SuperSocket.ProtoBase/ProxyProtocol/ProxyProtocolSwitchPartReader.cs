using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    class ProxyProtocolSwitchPartReader<TPackageInfo> : ProxyProtocolPackagePartReader<TPackageInfo>
    {
        private const int SWITCH_PART_SIZE = 12;

        private static readonly byte[] PROXYPROTOCOL_V2_SIGNATURE = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A, 0x00, 0x0D, 0x0A, 0x51, 0x55, 0x49, 0x54, 0x0A };
        
        private static readonly byte[] PROXY_TAG = Encoding.ASCII.GetBytes("PROXY TCP");

        public override bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            if (reader.Length < SWITCH_PART_SIZE)
            {
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            if (reader.IsNext(PROXYPROTOCOL_V2_SIGNATURE, true))
            {
                nextPartReader = ProxyProtocolV2Reader;
                return false;
            }

            int read = 0;

            try
            {
                if (TryReadProxyProtocolV1Tag(ref reader, out read))
                {
                    nextPartReader = ProxyProtocolV1Reader;
                    return false;
                }

                return true;
            }
            finally
            {
                if (read > 0)
                    reader.Rewind(read);
            }
        }

        private bool TryReadProxyProtocolV1Tag(ref SequenceReader<byte> reader, out int read)
        {
            read = 0;

            if (!reader.IsNext(PROXY_TAG, true))
            {
                return false;
            }

            read += PROXY_TAG.Length;

            if (!reader.TryRead(out var ver))
            {
                return false;
            }

            read++;

            if (ver != '4' && ver != '6')
            {
                return false;
            }

            if (!reader.TryRead(out var space))
            {
                return false;
            }

            read++;

            return space == ' ';
        }
    }
}
using System;
using System.Buffers;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    abstract class ProxyProtocolPackagePartReader<TPackageInfo> : IPackagePartReader<TPackageInfo>
    {
        static ProxyProtocolPackagePartReader()
        {
            ProxyProtocolSwitch = new ProxyProtocolSwitchPartReader<TPackageInfo>();
            ProxyProtocolV1Reader = new ProxyProtocolV1PartReader<TPackageInfo>();
            ProxyProtocolV2Reader =  new ProxyProtocolV2PartReader<TPackageInfo>();
        }

        public abstract bool Process(TPackageInfo package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData);
        
        internal static IPackagePartReader<TPackageInfo> ProxyProtocolSwitch { get; }

        internal static IPackagePartReader<TPackageInfo> ProxyProtocolV1Reader { get; }

        internal static IPackagePartReader<TPackageInfo> ProxyProtocolV2Reader { get; }
    }
}
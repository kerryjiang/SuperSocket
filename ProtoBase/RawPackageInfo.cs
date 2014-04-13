using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IRawPackageInfo
    {
        ReceiveCache RawData { get; }
    }

    public class RawPackageInfo<TKey> : IPackageInfo<TKey>, IRawPackageInfo
    {
        public TKey Key { get; private set; }

        public ReceiveCache RawData { get; private set; }

        public RawPackageInfo(TKey key, ReceiveCache rawData)
        {
            Key = key; ;
            RawData = rawData;
        }
    }
}

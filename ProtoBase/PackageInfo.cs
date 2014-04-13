using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class PackageInfo<TKey, TBody> : IPackageInfo<TKey>
    {
        public TKey Key { get; private set; }

        public TBody Body { get; private set; }

        public PackageInfo(TKey key, TBody body)
        {
            Key = key;
            Body = body;
        }
    }
}

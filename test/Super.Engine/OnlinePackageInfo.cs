using SuperSocket.ProtoBase;
using System;

namespace Super.Engine
{
    public class OnlinePackageInfo : IKeyedPackageInfo<short>
    {      
        public short Key { get; set; }
    
        public byte Version { get; set; }
      
        public Guid RequestID { get; set; }
       
        public int CrcCode { get; set; }
      
        public int BodyLength { get; set; }
      
        public object Object { get; set; }
    }

    public class OnlinePackageInfo<T> : OnlinePackageInfo
    {      
        public new T Object { get; set; }
    }
}

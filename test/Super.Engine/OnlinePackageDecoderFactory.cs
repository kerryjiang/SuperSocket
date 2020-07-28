using System.Collections.Generic;

namespace Super.Engine
{
    public class OnlinePackageDecoderFactory
    {
        private static readonly IDictionary<short, IDecoderFactory> _factorys = new Dictionary<short, IDecoderFactory>();
      
        public static void RegisterFactory(short key, IDecoderFactory onlinePackageFactory)
        {
            _factorys.TryAdd(key, onlinePackageFactory);
        }
       
        public static bool GetFactory(short key, out IDecoderFactory decoderFactory)
        {
            if (_factorys.TryGetValue(key, out IDecoderFactory factory))
            {
                decoderFactory = factory;
                return true;
            }
            else
            {
                decoderFactory = null;
                return false;
            }
        }
    }
}

using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace CommandServer
{
    public class StringPackageInfo : IKeyedPackageInfo<string>
    {
        public string Key { get; set; }

        public string[] Parameters { get; set; }
    }
}

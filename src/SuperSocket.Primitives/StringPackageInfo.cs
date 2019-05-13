using SuperSocket;

namespace SuperSocket
{
    public class StringPackageInfo : IKeyedPackageInfo<string>
    {
        public string Key { get; set; }

        public string Body { get; set; }

        public string[] Parameters { get; set; }
    }
}
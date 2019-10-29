using System.Security.Authentication;

namespace SuperSocket
{
    public class ListenOptions
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Path { get; set; }

        public int BackLog { get; set; }

        public bool NoDelay { get; set; }

        public SslProtocols Security { get; set; }
        

        public override string ToString()
        {
            return $"{nameof(Ip)}={Ip}, {nameof(Port)}={Port}, {nameof(Security)}={Security}, {nameof(Path)}={Path}, {nameof(BackLog)}={BackLog}, {nameof(NoDelay)}={NoDelay}";
        }
    }
}
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace CommandServer
{
    public class ADD : IAsyncCommand<string, StringPackageInfo>
    {
        public string Key => "ADD";

        public string Name => Key;

        public async Task ExecuteAsync(IAppSession session, StringPackageInfo package)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Sum();

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
        }
    }
}

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace CommandServer
{
    public class SUB : IAsyncCommand<StringPackageInfo>
    {
        public string Key => "SUB";

        public string Name => Key;

        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Aggregate((x, y) => x - y);

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"), cancellationToken);
        }
    }
}

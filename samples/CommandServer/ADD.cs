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
    [Command("add")]
    public class ADD : IAsyncCommand<StringPackageInfo>
    {
        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Sum();

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"), cancellationToken);
        }
    }
}

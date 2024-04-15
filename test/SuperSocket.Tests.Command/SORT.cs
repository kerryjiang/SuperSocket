using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Tests.Command
{
    public class SORT : IAsyncCommand<IAppSession, StringPackageInfo>
    {
        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {            
            var result = string.Join(' ', package.Parameters
                .Select(p => int.Parse(p)).OrderBy(x => x).Select(x => x.ToString()));

            await session.SendAsync(Encoding.UTF8.GetBytes($"{nameof(SORT)} {result}\r\n"), cancellationToken);
        }
    }
}

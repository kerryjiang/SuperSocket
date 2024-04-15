using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Tests.Command
{
    public class MIN : IAsyncCommand<IAppSession, StringPackageInfo>
    {
        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {            
            var result = package.Parameters
                .Select(p => int.Parse(p)).OrderBy(x => x).FirstOrDefault();

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"), cancellationToken);
        }
    }
}

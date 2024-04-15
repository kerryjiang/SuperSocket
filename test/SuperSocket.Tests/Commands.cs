using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using SuperSocket.Server;
using SuperSocket.Server.Host;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Tests.Command;
using System.Threading;


namespace SuperSocket.Tests
{
    public class MySession : AppSession
    {

    }
    
    public class ADD : IAsyncCommand<StringPackageInfo>
    {
        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Sum();

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
        }
    }

    public class MULT : IAsyncCommand<StringPackageInfo>
    {
        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Aggregate((x, y) => x * y);

            await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
        }
    }

    public class SUB : IAsyncCommand<StringPackageInfo>
    {
        private IPackageEncoder<string> _encoder;

        public SUB(IPackageEncoder<string> encoder)
        {
            _encoder = encoder;
        }

        public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var result = package.Parameters
                .Select(p => int.Parse(p))
                .Aggregate((x, y) => x - y);
            
            // encode the text message by encoder
            await session.SendAsync(_encoder, result.ToString() + "\r\n");
        }
    }

    public class DIV : IAsyncCommand<MySession, StringPackageInfo>
    {
        private IPackageEncoder<string> _encoder;

        public DIV(IPackageEncoder<string> encoder)
        {
            _encoder = encoder;
        }

        public async ValueTask ExecuteAsync(MySession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            var values = package
                .Parameters
                .Select(p => int.Parse(p))
                .ToArray();

            var result = values[0] / values[1];

            var socketSession = session as IAppSession;
            // encode the text message by encoder
            await socketSession.SendAsync(_encoder, result.ToString() + "\r\n", cancellationToken);
        }
    }

    public class PowData
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class POW : JsonAsyncCommand<IAppSession, PowData>
    {
        protected override async ValueTask ExecuteJsonAsync(IAppSession session, PowData data, CancellationToken cancellationToken)
        {
            await session.SendAsync(Encoding.UTF8.GetBytes($"{Math.Pow(data.X, data.Y)}\r\n"), cancellationToken);
        }
    }

    public class MaxData
    {
        public int[] Numbers { get; set; }
    }

    public class MAX : JsonAsyncCommand<IAppSession, MaxData>
    {
        protected override async ValueTask ExecuteJsonAsync(IAppSession session, MaxData data, CancellationToken cancellationToken)
        {
            var maxValue = data.Numbers.OrderByDescending(i => i).FirstOrDefault();
            await session.SendAsync(Encoding.UTF8.GetBytes($"{maxValue}\r\n"), cancellationToken);
        }
    }
}
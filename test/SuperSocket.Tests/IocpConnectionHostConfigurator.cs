using SuperSocket.IOCP.ConnectionFactory;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Tests;

public class IocpConnectionHostConfigurator : RegularHostConfigurator
{
    public IocpConnectionHostConfigurator()
        : base()
    {
    }

    public override void Configure(ISuperSocketHostBuilder hostBuilder)
    {
        base.Configure(hostBuilder);
        hostBuilder.UsePipeIocpConnection();
    }
}
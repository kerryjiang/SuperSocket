using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using System;
using System.Threading.Tasks;

namespace SuperSocket.Kestrel;

public sealed class KestrelChannelCreator : ConnectionHandler, IKestrelChannelCreator
{
    private readonly ILogger<KestrelChannelCreator> _logger;

    public ListenOptions Options => throw new NotImplementedException();

    public bool IsRunning { get; private set; }

    public event NewClientAcceptHandler NewClientAccepted;

    public event Func<ConnectionContext, ValueTask<IChannel>> ChannelFactoryAsync;

    public KestrelChannelCreator(ILogger<KestrelChannelCreator> logger)
    {
        _logger = logger;
    }

    Task<IChannel> IChannelCreator.CreateChannel(object connection) => throw new NotImplementedException();

    bool IChannelCreator.Start()
    {
        IsRunning = true;
        return true;
    }

    Task IChannelCreator.StopAsync()
    {
        IsRunning = false;
        return Task.CompletedTask;
    }

    public async override Task OnConnectedAsync(ConnectionContext connection)
    {
        var handler = NewClientAccepted;

        if (handler == null)
            return;

        IChannel channel = null;

        try
        {
            channel = await ChannelFactoryAsync(connection);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to create channel for {connection.RemoteEndPoint}.");
            await connection.DisposeAsync();
            return;
        }

        try
        {
            await handler.Invoke(this, channel);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
}

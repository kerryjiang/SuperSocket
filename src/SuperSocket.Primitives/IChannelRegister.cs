
using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IChannelRegister
    {
        void RegisterChannel(object connection);
    }
}
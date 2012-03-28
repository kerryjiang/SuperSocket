using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.Client.Config;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client
{
    class InProgressMessage
    {
        public string Target { get; set; }
    }

    abstract class EmptyMessageBase<T>
        where T : class, new()
    {
        public static T Empty { get; private set; }

        static EmptyMessageBase()
        {
            Empty = new T();
        }
    }

    class ExitMessage : EmptyMessageBase<ExitMessage>
    {
    }

    class ManageServersMessage : EmptyMessageBase<ManageServersMessage>
    {
    }

    class NewServerMessage : EmptyMessageBase<NewServerMessage>
    {
    }

    class CloseNewServerMessage : EmptyMessageBase<CloseNewServerMessage>
    {
    }

    class CloseEditServerMessage : EmptyMessageBase<CloseEditServerMessage>
    {
    }

    class NewEditServerMessage
    {
        public string Message { get; set; }
    }

    class ConfigCommandMessage
    {
        public ConfigCommandMessage(ServerViewModel server)
        {
            Server = server;
        }

        public ServerViewModel Server { get; private set; }
    }

    class NewServerCreatedMessage
    {
        public NewServerCreatedMessage(ServerConfig server)
        {
            Server = server;
        }

        public ServerConfig Server { get; private set; }
    }

    class ServerRemovedMessage
    {
        public ServerRemovedMessage(ServerConfig server)
        {
            Server = server;
        }

        public ServerConfig Server { get; private set; }
    }

    class AlertMessage
    {
        public AlertMessage(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}

using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : Attribute
    {
        public CommandFilterAttribute() : this(false)
        {
            
        }

        public CommandFilterAttribute(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public bool IsAsync { get; private set; }

        public abstract void OnCommandExecuting(IAppSession session, ICommand command);

        public abstract void OnCommandExecuted(IAppSession session, ICommand command);
    }
}


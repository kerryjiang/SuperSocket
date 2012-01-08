using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TAppSession, TRequestInfo> : ICommand
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession<TRequestInfo>
    {
        void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);
    }

    public class MockupCommand<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession<TRequestInfo>
    {
        public MockupCommand(string name)
        {
            Name = name;
        }

        public void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {

        }

        public string Name { get; private set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Utils;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ServerResource
{
    interface IResourceCreator<TDataType>
    {
        TDataType CreateResource(IServerConfig config);

        Action<TDataType> Assigner { get; set; }

        Func<TDataType> Accessor { get; set; }
    }

    abstract class ServerResourceItem : ITransactionItem
    {
        public string Name { get; private set; }

        protected ServerResourceItem(string name)
        {
            Name = name;
        }

        public abstract void Rollback();

        public static ITransactionItem Create<TResourceItemType, TDataType>(Action<TDataType> assignAction, IServerConfig config, Func<TDataType> accessFunc = null)
            where TResourceItemType : ServerResourceItem<TDataType>, ITransactionItem, IResourceCreator<TDataType>, new()
        {
            var resourceItem = new TResourceItemType();
            resourceItem.Assigner = assignAction;
            resourceItem.Accessor = accessFunc;
            TDataType resource = default(TDataType);

            try
            {
                resource = resourceItem.CreateResource(config);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to create the server resource '{0}'.", resourceItem.Name), e);
            }

            assignAction(resource);
            return resourceItem;
        }
    }

    abstract class ServerResourceItem<TDataType> : ServerResourceItem, ITransactionItem, IResourceCreator<TDataType>
    {
        private Action<TDataType> m_AssignAction;

        private Func<TDataType> m_AccessFunc;

        protected ServerResourceItem(string name)
            : base(name)
        {

        }

        TDataType IResourceCreator<TDataType>.CreateResource(IServerConfig config)
        {
            return CreateResource(config);
        }

        Action<TDataType> IResourceCreator<TDataType>.Assigner
        {
            get
            {
                return m_AssignAction;
            }
            set
            {
                m_AssignAction = value;
            }
        }

        Func<TDataType> IResourceCreator<TDataType>.Accessor
        {
            get
            {
                return m_AccessFunc;
            }
            set
            {
                m_AccessFunc = value;
            }
        }

        protected abstract TDataType CreateResource(IServerConfig config);

        public override void Rollback()
        {
            IDisposable prevResource = null;

            if (m_AccessFunc != null)
                prevResource = m_AccessFunc() as IDisposable;

            try
            {
                m_AssignAction(default(TDataType));
            }
            finally
            {
                if (prevResource != null)
                    prevResource.Dispose();
            }
        }
    }
}

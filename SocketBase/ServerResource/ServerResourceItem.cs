using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Utils;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ServerResource
{
    /// <summary>
    /// The resource creator interface
    /// </summary>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    public interface IResourceCreator<TDataType>
    {
        /// <summary>
        /// Creates the resource.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        TDataType CreateResource(IServerConfig config);

        /// <summary>
        /// Gets or sets the assigner of the resource.
        /// </summary>
        /// <value>
        /// The assigner.
        /// </value>
        Action<TDataType> Assigner { get; set; }

        /// <summary>
        /// Gets or sets the accessor of the resource.
        /// </summary>
        /// <value>
        /// The accessor.
        /// </value>
        Func<TDataType> Accessor { get; set; }
    }

    /// <summary>
    /// Server resource item
    /// </summary>
    public abstract class ServerResourceItem : ITransactionItem
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResourceItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected ServerResourceItem(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Rollbacks/cleans this resource.
        /// </summary>
        public abstract void Rollback();

        /// <summary>
        /// Creates the server resource item.
        /// </summary>
        /// <typeparam name="TResourceItemType">The type of the resource item type.</typeparam>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="assignAction">The assign action.</param>
        /// <param name="config">The config.</param>
        /// <param name="accessFunc">The access func.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
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

    /// <summary>
    /// Server resource item
    /// </summary>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    public abstract class ServerResourceItem<TDataType> : ServerResourceItem, ITransactionItem, IResourceCreator<TDataType>
    {
        private Action<TDataType> m_AssignAction;

        private Func<TDataType> m_AccessFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResourceItem{TDataType}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
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

        /// <summary>
        /// Creates the resource.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected abstract TDataType CreateResource(IServerConfig config);

        /// <summary>
        /// Rollbacks this resource.
        /// </summary>
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

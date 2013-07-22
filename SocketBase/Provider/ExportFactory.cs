using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Provider
{
    /// <summary>
    /// Export Factory
    /// </summary>
    [Serializable]
    public class ExportFactory
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string TypeName { get; set; }

        private Type m_LoadedType;

        [NonSerialized]
        private object m_Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFactory"/> class.
        /// </summary>
        public ExportFactory()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFactory"/> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public ExportFactory(object instance)
        {
            m_Instance = instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportFactory"/> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public ExportFactory(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Ensures the instance's existance.
        /// </summary>
        public void EnsureInstance()
        {
            if (m_Instance != null)
                return;

            m_Instance = CreateInstance();
        }

        private object CreateInstance()
        {
            if (m_LoadedType == null)
            {
                m_LoadedType = AssemblyUtil.GetType(TypeName, true, true);
            }

            return Activator.CreateInstance(m_LoadedType);
        }

        /// <summary>
        /// Creates the export type instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateExport<T>()
        {
            if (m_Instance != null)
                return (T)m_Instance;

            return (T)CreateInstance();
        }

        /// <summary>
        /// Creates the export type instance from the instance creator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator">The creator.</param>
        /// <returns></returns>
        public T CreateExport<T>(Func<Type, object> creator)
        {
            if (m_LoadedType == null)
            {
                m_LoadedType = AssemblyUtil.GetType(TypeName, true, true);
            }

            return (T)creator(m_LoadedType);
        }
    }
}

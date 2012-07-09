using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public Type Type { get; set; }

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
        /// <param name="type">The type.</param>
        public ExportFactory(Type type)
        {
            Type = type;
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

            if (Type != null)
                return (T)Activator.CreateInstance(Type);

            return default(T);
        }
    }
}

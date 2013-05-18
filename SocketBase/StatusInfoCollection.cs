using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Status information collection
    /// </summary>
    [Serializable]
    public class StatusInfoCollection
    {
        [NonSerialized]
        private Dictionary<string, object> m_Values = new Dictionary<string, object>();


        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public Dictionary<string, object> Values
        {
            get { return m_Values; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the collected time.
        /// </summary>
        /// <value>
        /// The collected time.
        /// </value>
        public DateTime CollectedTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Object" /> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object" />.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                object value;

                if (m_Values.TryGetValue(name, out value))
                    return value;

                return null;
            }
            set
            {
                m_Values[name] = value;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T GetValue<T>(string name, T defaultValue)
            where T : struct
        {
            object value;

            if (m_Values.TryGetValue(name, out value))
                return (T)value;

            return defaultValue;
        }

        private List<KeyValuePair<string, object>> m_InternalList;

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            m_InternalList = new List<KeyValuePair<string, object>>(m_Values.Count);

            foreach (var entry in m_Values)
            {
                m_InternalList.Add(new KeyValuePair<string, object>(entry.Key, entry.Value));
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (m_InternalList == null || m_InternalList.Count <= 0)
                return;

            if (m_Values == null)
                m_Values = new Dictionary<string, object>();

            foreach (var entry in m_InternalList)
            {
                m_Values.Add(entry.Key, entry.Value);
            }

            m_InternalList = null;
        }
    }
}

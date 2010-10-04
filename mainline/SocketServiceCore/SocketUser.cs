using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// The base class of application user
    /// </summary>
    [DataContract]
    public abstract class SocketUser
    {
        private long m_UserID;

        [DataMember]
        public long UserID
        {
            get { return m_UserID; }
            set { m_UserID = value; }
        }

        private string m_UserName;

        [DataMember]
        [XmlAttribute("UserName")]
        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        private string m_Password;

        [DataMember]
        [XmlAttribute("Password")]
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        private DateTime m_CreateTime;

        [DataMember]
        public DateTime CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }

        private DateTime m_LastLoginTime;

        [DataMember]
        public DateTime LastLoginTime
        {
            get { return m_LastLoginTime; }
            set { m_LastLoginTime = value; }
        }

        private int m_LoginTimes;

        [DataMember]
        public int LoginTimes
        {
            get { return m_LoginTimes; }
            set { m_LoginTimes = value; }
        }

        private bool m_Disabled;

        [DataMember]
        [XmlAttribute("Disabled")]
        public bool Disabled
        {
            get { return m_Disabled; }
            set { m_Disabled = value; }
        }
    }
}

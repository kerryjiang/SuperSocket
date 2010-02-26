using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.SocketServiceCore
{
	/// <summary>
	/// The base class of socketContext
	/// </summary>
	public abstract class SocketContext
	{
		private Encoding m_Charset = Encoding.Default;

		/// <summary>
		/// Gets or sets the charset.
		/// </summary>
		/// <value>The charset.</value>
		public Encoding Charset
		{
			get { return m_Charset; }
			set { m_Charset = value; }
		}

		private string m_UserName;

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		public string UserName
		{
			get { return m_UserName; }
			set { m_UserName = value; }
		}

		private bool m_Logged;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="SocketContext"/> is logged.
		/// </summary>
		/// <value><c>true</c> if logged; otherwise, <c>false</c>.</value>
		public bool Logged
		{
			get { return m_Logged; }
			set { m_Logged = value; }
		}

		private string m_prevCommand;

		/// <summary>
		/// Gets or sets the previous command.
		/// </summary>
		/// <value>The prev command.</value>
		public string PrevCommand
		{
			get { return m_prevCommand; }
			set { m_prevCommand = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this user is anonymous.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this user is anonymous; otherwise, <c>false</c>.
		/// </value>
		public virtual bool IsAnonymous
		{
			get
			{
				if (string.IsNullOrEmpty(m_UserName))
					return false;

				return string.Compare(m_UserName, "anonymous", StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

        private SocketContextStatus m_Status = SocketContextStatus.Healthy;

        public SocketContextStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        private string m_Message;

        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        public void SetError(string error)
        {
            m_Message = error;
            m_Status = SocketContextStatus.Error;
        }

        public void SetError(string error, params object[] paramValues)
        {
            m_Message = string.Format(error, paramValues);
            m_Status = SocketContextStatus.Error;
        }
	}
}

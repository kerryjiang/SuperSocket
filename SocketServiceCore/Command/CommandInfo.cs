using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
	public class CommandInfo
	{
		public CommandInfo()
		{
		}
		
		public CommandInfo(string cmdLine)
		{
			int pos = cmdLine.IndexOf(' ');

			if (pos > 0)
			{
				m_Name	= cmdLine.Substring(0, pos);
				m_Param	= cmdLine.Substring(pos + 1);
				m_Parmaters = m_Param.Split(' ');
			}
			else
			{
				m_Name = cmdLine;
			}
		}
		
		private string m_Tag = string.Empty;

		public string Tag
		{
			get { return m_Tag; }
			set { m_Tag = value; }
		}
		
		private string m_Name;

		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		private string m_Param;

		public string Param
		{
			get { return m_Param; }
			set { m_Param = value; }
		}

		private string[] m_Parmaters;

		public string[] Parameters
		{
			get { return m_Parmaters; }
			set { m_Parmaters = value; }
		}
		
		public string GetFirstParam()
		{
            return GetParamemterByIndex(0);
		}

        public string GetParamemterByIndex(int index)
        {
            if (m_Parmaters == null || m_Parmaters.Length <= index)
                return string.Empty;

            return m_Parmaters[index];
        }
	}
}

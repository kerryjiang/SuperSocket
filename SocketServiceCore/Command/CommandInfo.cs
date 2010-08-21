using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class CommandInfo
    {
        private CommandInfo()
        {
        }

        public CommandInfo(string name, string parameter)
        {
            this.m_Name = name;
            this.m_Param = parameter;
        }

        public CommandInfo(string name, string parameter, string tag)
            : this(name, parameter)
        {
            this.m_Tag = tag;
        }

        private bool m_ParametersInitialized = false;

        internal void InitializeParameters(string[] parameters)
        {
            if (m_ParametersInitialized)
                throw new Exception("Parameter array has been initialized, you shouldn't initialize it again!");

            m_Parmaters = parameters;
            m_ParametersInitialized = true;
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
        }

        private string m_Param;

        public string Param
        {
            get { return m_Param; }
        }

        private string[] m_Parmaters;

        public string[] Parameters
        {
            get { return m_Parmaters; }
        }

        public string GetFirstParam()
        {
            return GetParamemterByIndex(0);
        }

        private string GetParamemterByIndex(int index)
        {
            if (m_Parmaters == null || m_Parmaters.Length <= index)
                return string.Empty;

            return m_Parmaters[index];
        }

        public string this[int index]
        {
            get
            {
                return GetParamemterByIndex(index);
            }
        }
    }
}

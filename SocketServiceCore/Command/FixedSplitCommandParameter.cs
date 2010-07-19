using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class FixedSplitCommandParameter : ICommandParameterParser
    {
        private readonly int m_FixedParameterCount;

        public FixedSplitCommandParameter(int fixedParameterCount)
        {
            m_FixedParameterCount = fixedParameterCount;
        }

        #region ICommandParameterParser Members

        public string[] ParseCommandParameter(CommandInfo command)
        { 
            int currentIndex = 0;
            int startPos = 0;
            int pos = 0;

            List<string> paramList = new List<string>();

            while (currentIndex < m_FixedParameterCount - 1)
            {
                pos = command.Param.IndexOf(' ', startPos);

                if (pos < 0)
                    break;

                if (pos == startPos)
                {
                    startPos++;
                    continue;
                }

                paramList.Add(command.Param.Substring(startPos, pos - startPos));
                currentIndex++;
                startPos = pos + 1;

                if (startPos >= command.Param.Length)
                    break;
            }

            if (startPos < command.Param.Length)
            {
                paramList.Add(command.Param.Substring(startPos));
            }

            return paramList.ToArray();
        }

        #endregion
    }
}

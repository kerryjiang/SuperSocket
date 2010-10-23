using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class TagCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public CommandInfo ParseCommand(string command)
        {
            string name = string.Empty;
            string param = string.Empty;
            string tag = string.Empty;

            int tagPos = command.IndexOf(' ');

            if (tagPos > 0)
            {
                tag = command.Substring(0, tagPos);
                tagPos = tagPos + 1;
                int pos = 0;

                while (tagPos < command.Length)
                {
                    pos = command.IndexOf(' ', tagPos);

                    if (pos < 0)
                    {
                        name = command.Substring(tagPos);
                    }
                    else if(pos == tagPos)
                    {
                        tagPos++;
                        continue;
                    }
                    else
                    {
                        name = command.Substring(tagPos, pos - tagPos);
                        param = command.Substring(pos);
                    }
                }
                
                param = command.Substring(pos + 1);
            }
            else
            {
                tag = command;
            }

            return new CommandInfo(name, param, tag);
        }

        #endregion
    }
}

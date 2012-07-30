using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.Threading.Tasks;
using System.IO;

namespace SuperSocket.Test.Command
{
    public class SEND : StringCommandBase<TestSession>
    {
        public static string[] GetStringSource()
        {
            var filePath = Path.Combine(Path.GetDirectoryName(new Uri(typeof(SEND).Assembly.CodeBase).LocalPath), "Strings.txt");

            var list = new List<string>();

            using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                while(true)
                {
                    var line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    list.Add(line);
                }

                reader.Close();
            }

            return list.ToArray();
        }

        public override void ExecuteCommand(TestSession session, StringRequestInfo commandData)
        {
            string[] source = GetStringSource();

            Parallel.For(0, source.Length, (i) =>
                {
                    session.Send(source[i]);
                });
        }
    }
}

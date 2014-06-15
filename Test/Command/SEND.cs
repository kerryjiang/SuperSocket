using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;
using System.Threading.Tasks;
using System.IO;

namespace SuperSocket.Test.Command
{
    public class SEND : StringCommandBase<TestSession>
    {
        public static string[] GetStringSource()
        {
            var list = new List<string>();

            using (var reader = new StringReader(Setup.GetResourceContent("Strings.txt")))
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

        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            string[] source = GetStringSource();

            Parallel.For(0, source.Length, (i) =>
                {
                    session.Send(source[i]);
                });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Logging;
using System.Threading;
using System.Reflection;
using System.IO;
using SuperSocket.SocketBase;

namespace SuperSocket.Test
{
    public static class Setup
    {
        public static string GetResourceContent(string fileName)
        {
            var currentAssembly = typeof(Setup).Assembly;
            var resourceName = currentAssembly.GetName().Name + "." + fileName.Replace(Path.DirectorySeparatorChar, '.');

            using (var reader = new StreamReader(currentAssembly.GetManifestResourceStream(resourceName), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }

    class ConsoleWriter : StreamWriter
    {
        public ConsoleWriter(Stream stream, Encoding encoding, int bufferSize)
            : base(stream, encoding, bufferSize)
        {

        }

        private const string m_NewLine = "\r\n";

        public override void WriteLine()
        {
            Write(m_NewLine);
        }
    }
}

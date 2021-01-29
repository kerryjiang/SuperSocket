using System.IO;
using System.Text;

namespace SuperSocket.Tests
{
    public class ConsoleWriter : StreamWriter
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
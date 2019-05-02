using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace CommandServer
{
    class CommandLinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
    {
        public CommandLinePipelineFilter()
            : base(new[] { (byte)'\r', (byte)'\n' })
        {

        }

        protected override StringPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
        {
            var text = buffer.GetString(Encoding.UTF8);
            var parts = text.Split(' ');

            return new StringPackageInfo
            {
                Key = parts[0],
                Parameters = parts.Skip(1).ToArray()
            };
        }
    }
}

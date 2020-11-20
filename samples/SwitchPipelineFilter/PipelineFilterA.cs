using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SwitchPipelineFilter
{
    public class PipelineFilterA : BeginEndMarkPipelineFilter<TextPackageInfo>
    {
        private static byte[] _beginMark = new byte[] { (byte)'Y' };
        private static byte[] _endMark = new byte[] { 0x00, 0xff };

        public IPipelineFilter<TextPackageInfo> SwitchFilter { get; }

        public PipelineFilterA(IPipelineFilter<TextPackageInfo> switcher)
            : base(_beginMark, _endMark)
        {
            SwitchFilter = switcher;
        }

        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            NextFilter = SwitchFilter;
            return base.DecodePackage(ref buffer);
        }
    }
}

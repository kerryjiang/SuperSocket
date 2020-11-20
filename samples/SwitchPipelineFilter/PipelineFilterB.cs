using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SwitchPipelineFilter
{
    public class PipelineFilterB : BeginEndMarkPipelineFilter<TextPackageInfo>
    {
        private static byte[] _beginMark = new byte[] { (byte)'*' };
        private static byte[] _endMark = new byte[] { (byte)'#' };

        public IPipelineFilter<TextPackageInfo> SwitchFilter { get; }

        public PipelineFilterB(IPipelineFilter<TextPackageInfo> switcher)
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

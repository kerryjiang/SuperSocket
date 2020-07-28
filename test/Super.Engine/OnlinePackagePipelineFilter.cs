using SuperSocket.ProtoBase;

namespace Super.Engine
{
    public class OnlinePackagePipelineFilter : TerminatorPipelineFilter<OnlinePackageInfo>
    {
        static readonly byte[] _terminator = new byte[] { 0xFE, 0xEF, 0xFD, 0xDF, 0xFC, 0xCF, 0xFE, 0xEF, 0xFD, 0xDF, 0xFC, 0xCF };

        public OnlinePackagePipelineFilter()
            : base(_terminator)
        {

        }
    }
}

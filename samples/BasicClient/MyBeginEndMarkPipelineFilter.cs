using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicClient
{
    public class MyBeginEndMarkPipelineFilter : BeginEndMarkPipelineFilter<StringPackageInfo>
    {
        //Both begin mark and end mark can be two or more bytes
        public readonly static byte[] DefaultBeginMark = new byte[] { 11 }; // HEX 0B
        public readonly static byte[] DefaultEndMark = new byte[] { 28, 13 }; // HEX 1C, 0D

        public ReadOnlyMemory<byte> BeginMark { get; }
        public ReadOnlyMemory<byte> EndMark { get; }
        public MyBeginEndMarkPipelineFilter()
            : base(new ReadOnlyMemory<byte>(DefaultBeginMark), new ReadOnlyMemory<byte>(DefaultBeginMark))
        {
            BeginMark = DefaultBeginMark;
            EndMark = DefaultEndMark;
        }
        public MyBeginEndMarkPipelineFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark)
            : base(beginMark, endMark)
        {
            BeginMark = beginMark;
            EndMark = endMark;
        }
    }
}

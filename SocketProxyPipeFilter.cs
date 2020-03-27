using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// 握手
    /// </summary>
    public class Socket5AuthenticateHandshakePipeFilter : BaseProxyPipeFilter
    {
        public Socket5AuthenticateHandshakePipeFilter() : base(2, new Socket5AuthenticateUserNamePipeFilter())
        {
        }
    }

    /// <summary>
    /// 账号密码
    /// </summary>
    public class Socket5AuthenticateUserNamePipeFilter : BaseProxyPipeFilter
    {
        public Socket5AuthenticateUserNamePipeFilter() : base(2, new Socket5AuthenticateEndPointPipeFilter())
        {
        }
    }

    /// <summary>
    /// ip地址
    /// </summary>
    public class Socket5AuthenticateEndPointPipeFilter : BaseProxyPipeFilter
    {
        public Socket5AuthenticateEndPointPipeFilter() : base(4)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HttpPipeLineFilter : IPipelineFilter<ProxyDataPackageInfo>
    {
        readonly static ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        public IPackageDecoder<ProxyDataPackageInfo> Decoder { get; set; }

        public IPipelineFilter<ProxyDataPackageInfo> NextFilter { get; internal set; }

        public object Context { get; set; }

        public ProxyDataPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var terminatorSpan = _headerTerminator.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
                return null;

            reader.Advance(terminatorSpan.Length);

            return new ProxyDataPackageInfo { Data = pack.ToArray() };
        }

        public void Reset()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseProxyPipeFilter : FixedSizePipelineFilter<ProxyDataPackageInfo>
    {
        private int _receiverCount;

        public BaseProxyPipeFilter(int receiverCount, IPipelineFilter<ProxyDataPackageInfo> pipelineFilter = null) : base(receiverCount)
        {
            base.NextFilter = pipelineFilter;
            _receiverCount = receiverCount;
        }

        protected override ProxyDataPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
        {
            var date = buffer.Slice(0, _receiverCount).ToArray();

            return new ProxyDataPackageInfo { Data = date };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProxyDataPackageInfo
    {
        public byte[] Data { get; set; }
    }
}

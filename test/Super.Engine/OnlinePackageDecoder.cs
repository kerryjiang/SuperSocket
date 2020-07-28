using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using Volo.Abp.DependencyInjection;

namespace Super.Engine
{
    [ExposeServices(typeof(IPackageDecoder<OnlinePackageInfo>))]
    public class OnlinePackageDecoder : IPackageDecoder<OnlinePackageInfo>, ISingletonDependency
    {
        private const int HEADER_LENGTH = 29;

        private readonly ILogger<OnlinePackageDecoder> _logger;

        public IMemoryCache MemoryCache { get; set; }

        public OnlinePackageDecoder(ILogger<OnlinePackageDecoder> logger)
        {
            _logger = logger;
        }
     
        public OnlinePackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
        {
            if (buffer.Length < HEADER_LENGTH)
                throw new Exception($"proto error:head lenth less than { HEADER_LENGTH }");

            try
            {
                var reader = new SequenceReader<byte>(buffer);

                var packgae = new OnlinePackageInfo();
                reader.Advance(1);

                if (reader.TryReadBigEndian(out short key))
                    packgae.Key = key;

                reader.Advance(1);

                if (reader.TryRead(out byte version))
                    packgae.Version = version;

                packgae.RequestID = new Guid(reader.CurrentSpan.Slice(6, 16));
                reader.Advance(16);

                if (reader.TryReadLittleEndian(out int crcCode))
                    packgae.CrcCode = crcCode;

                if (reader.TryReadLittleEndian(out int bodyLength))
                    packgae.BodyLength = bodyLength;

                if (buffer.Length != packgae.BodyLength + HEADER_LENGTH)
                    throw new Exception($"Package error:length{ packgae.BodyLength },active length{ buffer.Length - HEADER_LENGTH } ");

                if (packgae.BodyLength > 0)
                {
                    if (OnlinePackageDecoderFactory.GetFactory(packgae.Key, out IDecoderFactory onlinePackageFactory))
                    {
                        var sclie = buffer.Slice(HEADER_LENGTH, packgae.BodyLength);
                        packgae.Object = onlinePackageFactory.Create(ref sclie);
                    }
                    else
                    {
                        _logger.LogError("Not find the command");
                    }
                }

                return packgae;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default;
            }
        }
    }
}

using System;
using System.Buffers;

namespace Super.Engine
{
    public interface IDecoderFactory
    {       
        object Create(ref ReadOnlySequence<byte> buffer);
    }

    public class DecoderFactory<T> : IDecoderFactory
    {
        public DecoderFactory()
        {
            if (typeof(T) != typeof(ReadOnlySequence<byte>))
                ProtoBuf.Serializer.PrepareSerializer<T>();
        }

        public object Create(ref ReadOnlySequence<byte> buffer)
        {
            if (typeof(T) == typeof(ReadOnlySequence<byte>))
            {
                return buffer;
            }

            if (typeof(T).IsValueType)
            {
                throw new NotSupportedException($"{typeof(T).Name} has not support");
            }

            return Get(ref buffer);
        }
     
        T Get(ref ReadOnlySequence<byte> buffer) => ProtoBuf.Serializer.Deserialize<T>(buffer);
    }
}

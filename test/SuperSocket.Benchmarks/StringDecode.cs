using System;
using System.Buffers;
using System.Text;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using SuperSocket.ProtoBase;

namespace SuperSocket.Benchmarks
{
    [MemoryDiagnoser]
    public class StringDecode
    {
        [Params(10, 100, 1000)]
        public int N;

        [Params(1, 3, 5, 10, 50)]
        public int S;

        private Encoding _encoding = Encoding.ASCII;
        private SequenceSegment _head;
        private SequenceSegment _tail;


        [GlobalSetup]
        public void GlobalSetup()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < S; i++)
            {
                while (true)
                {
                    sb.Append(Guid.NewGuid().ToString().Replace("-", string.Empty));

                    if (sb.Length >= N)
                    {
                        var data = _encoding.GetBytes(sb.ToString(0, N));

                        var segment = new SequenceSegment(data, data.Length);

                        if (_head == null)
                            _tail = _head = segment;
                        else
                            _tail = _tail.SetNext(segment);

                        sb.Clear();
                        break;
                    }
                }
            }

                  
        }


        [Benchmark]
        public string DecodeDirect()
        {
            var dataSequence = new ReadOnlySequence<byte>(_head, 0, _tail, _tail.Memory.Length);
            return _encoding.GetString(dataSequence.ToArray());
        }

        [Benchmark]
        public string DecodePieceByPiece()
        {
            var dataSequence = new ReadOnlySequence<byte>(_head, 0, _tail, _tail.Memory.Length);
            return dataSequence.GetString(_encoding);
        }
    }
}

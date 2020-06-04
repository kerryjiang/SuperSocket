using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BasicClient
{
    public class MesMessagePipelineFilter : PipelineFilterBase<MesMessage>
    {
        private readonly byte leftEmbrace = Convert.ToByte('{');
        private readonly byte rightEmbrace = Convert.ToByte('}');
        private readonly byte leftBracket = Convert.ToByte('[');
        private readonly byte rightBracket = Convert.ToByte(']');
        private readonly byte escape = Convert.ToByte('\\');
        private byte _header;

        private int leftEmbraceCount;
        private int rightEmbraceCount;
        private int leftBracketCount;
        private int rightBracketCount;
        private bool lastOneIsEscape;
        private List<byte> localList = new List<byte>();

        public override MesMessage Filter(ref SequenceReader<byte> reader)
        {

            if (_header == 0x00)
            {
                int indexHeader;

                var indexLeftEmbrace = reader.CurrentSpan.IndexOf(leftEmbrace);
                var indexLeftBracket = reader.CurrentSpan.IndexOf(leftBracket);
                if (indexLeftEmbrace < 0 && indexLeftBracket < 0)
                {
                    var remained = reader.CurrentSpan.Length;
                    reader.Advance(remained);
                    return null;
                }
                else if (indexLeftBracket < 0 && indexLeftEmbrace >= 0)
                {
                    indexHeader = indexLeftEmbrace;
                    _header = leftEmbrace;
                }
                else if (indexLeftEmbrace < 0 && indexLeftBracket >= 0)
                {
                    indexHeader = indexLeftBracket;
                    _header = leftBracket;
                }
                else
                {
                    if (indexLeftEmbrace < indexLeftBracket)
                    {
                        indexHeader = indexLeftEmbrace;
                        _header = leftEmbrace;
                    }
                    else
                    {
                        indexHeader = indexLeftBracket;
                        _header = leftBracket;
                    }
                }
                reader.Advance(indexHeader);
            }

            while (reader.TryRead(out byte current))
            {
                localList.Add(current);
                if (!lastOneIsEscape)
                {
                    if (current == escape)
                        lastOneIsEscape = true;
                    else if (current == leftEmbrace)
                        leftEmbraceCount++;
                    else if (current == leftBracket)
                        leftBracketCount++;
                    else if (current == rightEmbrace)
                        rightEmbraceCount++;
                    else if (current == rightBracket)
                        rightBracketCount++;
                }
                else
                {
                    lastOneIsEscape = false;
                }
                if (leftEmbraceCount == rightEmbraceCount && leftBracketCount == rightBracketCount)
                {
                    if (leftEmbraceCount == 0 && leftBracketCount == 0)
                        continue;
                    else
                    {
                        break;
                    }
                }
            }
            try
            {
                var package = JsonSerializer.Deserialize<MesMessage>(localList.ToArray());
                return package;
            }
            catch (Exception ex)
            {
                Reset();
                Console.WriteLine(ex);
                return null;
            }
        }

        public override void Reset()
        {
            _header = 0x00;

            leftEmbraceCount = 0;
            rightEmbraceCount = 0;
            leftBracketCount = 0;
            rightBracketCount = 0;
            lastOneIsEscape = false;
            localList.Clear();
        }
    }
}

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MySQL.FramePartReader
{
    /// <summary>
    /// https://dev.mysql.com/doc/dev/mysql-server/8.0.11/page_protocol_com_query_response.html
    /// </summary>
    sealed class PackageHeadReader : DataFramePartReader
    {
        public override bool Process(QueryResult package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            var firstByte = reader.TryRead();

            if (firstByte == 0xFF)
            {
                needMoreData = false;
                nextPartReader = ErrorCodePartReader;
                return false;
            }
            else if (firstByte == 0x00)
            {
                throw new NotSupportedException();
            }
            else if (firstByte == 0xFB)
            {
                throw new NotSupportedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

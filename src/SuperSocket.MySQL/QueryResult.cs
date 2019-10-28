using System;
using System.Collections.Generic;
using System.Buffers;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.MySQL
{
    /// <summary>
    /// https://dev.mysql.com/doc/dev/mysql-server/8.0.11/page_protocol_com_query.html
    /// </summary>
    public class QueryResult
    {
        public short ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string[] Columns { get; set; }

        public List<string[]> Rows { get; set; }
    }
}
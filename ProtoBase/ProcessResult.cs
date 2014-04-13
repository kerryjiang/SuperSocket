using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public struct ProcessResult
    {
        public ProcessState State { get; private set; }

        public string Message { get; private set; }

        public static ProcessResult Create(ProcessState state)
        {
            var result = new ProcessResult();
            result.State = state;
            return result;
        }

        public static ProcessResult Create(ProcessState state, string message)
        {
            var result = new ProcessResult();
            result.State = state;
            result.Message = message;
            return result;
        }
    }
}

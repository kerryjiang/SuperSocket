using System;

namespace BasicClient
{
    public enum OpCode : byte
    {
        Connect = 1,
        Subscribe = 2,
        Publish = 3
    }

    public class MyPackage
    {
        public OpCode Code { get; set; }

        public short Sequence { get; set; }

        public string Body { get; set; }
    }
}
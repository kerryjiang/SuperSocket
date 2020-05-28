using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace BasicClient
{
    public enum RTDECommandEnum : byte
    {
        Connect = 1,
        Subscribe = 2,
        Publish = 3
    }
    public class RTDEPackage
    {
        public RTDECommandEnum RoboCmd { get; set; }
        public string Data { get; set; }
        public byte[] DataBytes { get; set; }
        
        public RTDEPackage(RTDECommandEnum cmd, string data)
        {
            RoboCmd = cmd;
            Data = data;
            DataBytes = Encoding.UTF8.GetBytes(data);
        }
        public RTDEPackage(RTDECommandEnum cmd, byte[] data)
        {
            RoboCmd = cmd;
            DataBytes = data;
            Data = Encoding.UTF8.GetString(data);
        }
        public override string ToString()
        {
            return $"{RoboCmd}: {Data}";
        }
    }
}

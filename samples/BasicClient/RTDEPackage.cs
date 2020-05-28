using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace BasicClient
{
    public enum RoboCmd : byte
    {
        Connect = 1,
        Subscribe = 2,
        Publish = 3
    }
    public class RTDEPackage  
    {
        public RoboCmd RoboCmd { get; set; }
        public string Data { get; set; }
    }
}

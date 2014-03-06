using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class StringPackageInfo : IPackageInfo<string>
    {
        public string Key { get; private set; }

        public string Body { get; private set; }

        public string[] Parameters { get; private set; }

        public StringPackageInfo(string key, string body, string[] parameters)
        {
            Key = key;
            Body = body;
            Parameters = parameters;
        }
    }
}

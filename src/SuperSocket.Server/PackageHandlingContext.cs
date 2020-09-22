using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server
{
    public class PackageHandlingContext<TAppSession, TPackageInfo>
    {
        public PackageHandlingContext(TAppSession appSession, TPackageInfo packageInfo)
        {
            AppSession = appSession;
            PackageInfo = packageInfo;
        }

        public TAppSession AppSession { get; }

        public TPackageInfo PackageInfo { get; }
    }
}

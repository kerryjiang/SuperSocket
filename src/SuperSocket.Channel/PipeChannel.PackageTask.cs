using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase;
using System.Collections;
using System.Threading.Tasks.Sources;
using System.Threading;

namespace SuperSocket.Channel
{
    public abstract partial class PipeChannel<TPackageInfo> : IValueTaskSource<TPackageInfo>, IValueTaskSource
        where TPackageInfo : class
    {        

        private ManualResetValueTaskSourceCore<TPackageInfo> _packageTaskSource;

        void SetResult(TPackageInfo package)
        {
            _packageTaskSource.SetResult(package);
        }

        TPackageInfo IValueTaskSource<TPackageInfo>.GetResult(short token)
        {
            return _packageTaskSource.GetResult(token);
        }

        void IValueTaskSource.GetResult(short token)
        {
            _packageTaskSource.GetResult(token);
        }

        ValueTaskSourceStatus IValueTaskSource<TPackageInfo>.GetStatus(short token)
        {
            return _packageTaskSource.GetStatus(token);
        }

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            return _packageTaskSource.GetStatus(token);
        }

        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _packageTaskSource.OnCompleted(continuation, state, token, flags);
        }

        void IValueTaskSource<TPackageInfo>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _packageTaskSource.OnCompleted(continuation, state, token, flags);
        }

        private ValueTask<TPackageInfo> ReceivePackage()
        {
            return new ValueTask<TPackageInfo>(this, _packageTaskSource.Version);
        }
    }
}

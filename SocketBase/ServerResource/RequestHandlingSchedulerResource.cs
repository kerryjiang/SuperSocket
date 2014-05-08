using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Scheduler;

namespace SuperSocket.SocketBase.ServerResource
{
    class RequestHandlingSchedulerResource : ServerResourceItem<TaskScheduler>
    {
        public RequestHandlingSchedulerResource()
            : base("RequestHandlingScheduler")
        {

        }

        protected override TaskScheduler CreateResource(IServerConfig config)
        {
            if (config.RequestHandlingMode == RequestHandlingMode.SingleThread)
                return new SingleThreadTaskScheduler();

            if (config.RequestHandlingMode == RequestHandlingMode.CustomThreadPool)
                return new CustomThreadPoolTaskScheduler(config);

            return null;
        }
    }
}

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
            {
                if (config.RequestHandlingThreads <= 1 || config.RequestHandlingThreads > 100)
                    throw new Exception("RequestHandlingThreads must be between 2 and 100!");

                return new CustomThreadPoolTaskScheduler(config.RequestHandlingThreads);
            }

            return null;
        }
    }
}

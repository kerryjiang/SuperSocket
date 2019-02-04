using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IListenerFactory
    {
        IListener CreateListener(ListenOptions options);
    }
}
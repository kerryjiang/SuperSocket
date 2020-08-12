using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public interface ICommandSource
    {
        IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketBase.Command
{
    class CommandInfo<TCommand>
        where TCommand : ICommand
    {
        public TCommand Command { get; private set; }

        public CommandFilterAttribute[] Filters { get; private set; }

        public CommandInfo(TCommand command, IEnumerable<CommandFilterAttribute> globalFilters)
        {
            Command = command;

            var allFilters = new List<CommandFilterAttribute>();

            if (globalFilters != null && globalFilters.Any())
            {
                allFilters.AddRange(globalFilters);
            }

            IEnumerable<CommandFilterAttribute> filters = command is ICommandFilterProvider ?
                (command as ICommandFilterProvider).GetFilters() : AppServer.GetCommandFilterAttributes(command.GetType());

            if (filters != null && filters.Any())
                allFilters.AddRange(filters);

            if (allFilters.Any())
            {
                Filters = allFilters.OrderBy(f => f.Order).ToArray();
            }
        }
    }
}

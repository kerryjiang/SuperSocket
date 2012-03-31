using System;
using System.Collections.Generic;
using System.Linq;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Command filter factory
    /// </summary>
    public static class CommandFilterFactory
    {
        /// <summary>
        /// Generates the command filter library.
        /// </summary>
        /// <param name="serverType">Type of the server.</param>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public static Dictionary<string, List<CommandFilterAttribute>> GenerateCommandFilterLibrary(Type serverType, IEnumerable<ICommand> commands)
        {
            var library = new Dictionary<string, List<CommandFilterAttribute>>(StringComparer.OrdinalIgnoreCase);
            
            //Get global command filters
            var globalFilters = GetFilterAttributes(serverType);
            
            foreach(var command in commands)
            {
                //Get command specified filters
                var commandAttrs = GetFilterAttributes(command.GetType());
                var applyAttrs = new List<CommandFilterAttribute>(commandAttrs.Length + globalFilters.Length);
                if(globalFilters.Length > 0)
                    applyAttrs.AddRange(globalFilters);
                if(commandAttrs.Length > 0)
                    applyAttrs.AddRange(commandAttrs);
                if(applyAttrs.Count > 0)
                    library.Add(command.Name, applyAttrs);
            }
            
            return library;
        }

        /// <summary>
        /// Gets the filter attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static CommandFilterAttribute[] GetFilterAttributes(Type type)
        {
            var attrs = type.GetCustomAttributes(true);
            return attrs.OfType<CommandFilterAttribute>().ToArray();
        }
    }
}


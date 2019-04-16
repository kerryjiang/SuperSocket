using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public class CommandOptions : ICommandSource
    {
        public CommandAssemblyConfig[] Assemblies { get; set; }

        public List<ICommandSource> CommandSources { get; set; }

        public IEnumerable<Type> GetCommandTypes()
        {
            throw new NotImplementedException();
        }
    }

    public class CommandAssemblyConfig : ICommandSource
    {
        public string Name { get; set; }

        public IEnumerable<Type> GetCommandTypes()
        {
            throw new NotImplementedException();
        }
    }

    public class ActualCommandAssembly : ICommandSource
    {
        public Assembly Assembly { get; set; }

        public IEnumerable<Type> GetCommandTypes()
        {
            throw new NotImplementedException();
        }
    }

    public class ActualCommand : ICommandSource
    {
        public Type CommandType { get; set; }

        public IEnumerable<Type> GetCommandTypes()
        {
            yield return CommandType;
        }
    }
}

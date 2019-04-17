using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public class CommandOptions : ICommandSource
    {
        public CommandOptions()
        {
            CommandSources = new List<ICommandSource>();
        }

        public CommandAssemblyConfig[] Assemblies { get; set; }

        public List<ICommandSource> CommandSources { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> critera)
        {
            var commandSources = CommandSources;
            var configuredAssemblies = Assemblies;

            if (configuredAssemblies != null && configuredAssemblies.Any())
            {
                commandSources.AddRange(configuredAssemblies);
            }

            var commandTypes = new List<Type>();

            foreach (var source in commandSources)
            {
                commandTypes.AddRange(source.GetCommandTypes(critera));
            }

            return commandTypes;
        }
    }

    public class CommandAssemblyConfig : AssemblyBaseCommandSource, ICommandSource
    {
        public string Name { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> critera)
        {
            return GetCommandTypesFromAssembly(Assembly.Load(Name)).Where(t => critera(t));
        }
    }

    public class ActualCommandAssembly : AssemblyBaseCommandSource, ICommandSource
    {
        public Assembly Assembly { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> critera)
        {
            return GetCommandTypesFromAssembly(Assembly).Where(t => critera(t));
        }
    }

    public abstract class AssemblyBaseCommandSource
    {
        public IEnumerable<Type> GetCommandTypesFromAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes();
        }
    }

    public class ActualCommand : ICommandSource
    {
        public Type CommandType { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> critera)
        {
            if (critera(CommandType))
                yield return CommandType;
        }
    }

    public static class CommandOptionsExtensions
    {
        public static void AddCommand<TCommand>(this CommandOptions commandOptions)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = typeof(TCommand) });
        }

        public static void AddCommand(this CommandOptions commandOptions, Type commandType)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = commandType });
        }

        public static void AddCommandAssembly(this CommandOptions commandOptions, Assembly commandAssembly)
        {
            commandOptions.CommandSources.Add(new ActualCommandAssembly { Assembly = commandAssembly });
        }
    }
}

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
            _globalCommandFilterTypes = new List<Type>();
        }

        public CommandAssemblyConfig[] Assemblies { get; set; }

        public List<ICommandSource> CommandSources { get; set; }

        private List<Type> _globalCommandFilterTypes;

        public IReadOnlyList<Type> GlobalCommandFilterTypes
        {
            get { return _globalCommandFilterTypes; }
        }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
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
                commandTypes.AddRange(source.GetCommandTypes(criteria));
            }

            return commandTypes;
        }

        internal void AddGlobalCommandFilterType(Type commandFilterType)
        {
            _globalCommandFilterTypes.Add(commandFilterType);
        }
    }

    public class CommandAssemblyConfig : AssemblyBaseCommandSource, ICommandSource
    {
        public string Name { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly.Load(Name)).Where(t => criteria(t));
        }
    }

    public class ActualCommandAssembly : AssemblyBaseCommandSource, ICommandSource
    {
        public Assembly Assembly { get; set; }

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly).Where(t => criteria(t));
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

        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            if (criteria(CommandType))
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

        public static void AddGlobalCommandFilter<TCommandFilter>(this CommandOptions commandOptions)
            where TCommandFilter : CommandFilterBaseAttribute
        {
            commandOptions.AddGlobalCommandFilterType(typeof(TCommandFilter));
        }

        public static void AddGlobalCommandFilter(this CommandOptions commandOptions, Type commandFilterType)
        {
            if (!typeof(CommandFilterBaseAttribute).IsAssignableFrom(commandFilterType))
                throw new Exception("The command filter type must inherit CommandFilterBaseAttribute.");

            commandOptions.AddGlobalCommandFilterType(commandFilterType);
        }
    }
}

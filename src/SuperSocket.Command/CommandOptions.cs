using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;
using System.Threading;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents options for configuring commands in a SuperSocket application.
    /// </summary>
    public class CommandOptions : ICommandSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOptions"/> class.
        /// </summary>
        public CommandOptions()
        {
            CommandSources = new List<ICommandSource>();
            _globalCommandFilterTypes = new List<Type>();
        }

        /// <summary>
        /// Gets or sets the assemblies containing commands.
        /// </summary>
        public CommandAssemblyConfig[] Assemblies { get; set; }

        /// <summary>
        /// Gets or sets the list of command sources.
        /// </summary>
        public List<ICommandSource> CommandSources { get; set; }

        private List<Type> _globalCommandFilterTypes;

        /// <summary>
        /// Gets the list of global command filter types.
        /// </summary>
        public IReadOnlyList<Type> GlobalCommandFilterTypes => _globalCommandFilterTypes;

        /// <summary>
        /// Registers a handler for unknown packages.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package.</typeparam>
        /// <param name="unknownPackageHandler">The handler for unknown packages.</param>
        public void RegisterUnknownPackageHandler<TPackageInfo>(Func<IAppSession, TPackageInfo, CancellationToken, ValueTask> unknownPackageHandler)
        {
            UnknownPackageHandler = unknownPackageHandler;
        }

        internal object UnknownPackageHandler { get; private set; }

        /// <summary>
        /// Retrieves command types that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria to filter command types.</param>
        /// <returns>An enumerable collection of command types.</returns>
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

    /// <summary>
    /// Represents a configuration for a command assembly.
    /// </summary>
    public class CommandAssemblyConfig : AssemblyBaseCommandSource, ICommandSource
    {
        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Retrieves command types from the assembly that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria to filter command types.</param>
        /// <returns>An enumerable collection of command types.</returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly.Load(Name)).Where(t => criteria(t));
        }
    }

    /// <summary>
    /// Represents an actual command assembly.
    /// </summary>
    public class ActualCommandAssembly : AssemblyBaseCommandSource, ICommandSource
    {
        /// <summary>
        /// Gets or sets the assembly containing commands.
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// Retrieves command types from the assembly that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria to filter command types.</param>
        /// <returns>An enumerable collection of command types.</returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            return GetCommandTypesFromAssembly(Assembly).Where(t => criteria(t));
        }
    }

    /// <summary>
    /// Represents a base class for retrieving command types from an assembly.
    /// </summary>
    public abstract class AssemblyBaseCommandSource
    {
        /// <summary>
        /// Retrieves all exported types from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to retrieve types from.</param>
        /// <returns>An enumerable collection of exported types.</returns>
        public IEnumerable<Type> GetCommandTypesFromAssembly(Assembly assembly)
        {
            return assembly.GetExportedTypes();
        }
    }

    /// <summary>
    /// Represents an actual command with a specific type.
    /// </summary>
    public class ActualCommand : ICommandSource
    {
        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        public Type CommandType { get; set; }

        /// <summary>
        /// Retrieves the command type if it matches the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria to filter command types.</param>
        /// <returns>An enumerable collection containing the command type if it matches the criteria.</returns>
        public IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria)
        {
            if (criteria(CommandType))
                yield return CommandType;
        }
    }

    /// <summary>
    /// Provides extension methods for configuring command options.
    /// </summary>
    public static class CommandOptionsExtensions
    {
        /// <summary>
        /// Adds a command to the command options.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to add.</typeparam>
        /// <param name="commandOptions">The command options to configure.</param>
        public static void AddCommand<TCommand>(this CommandOptions commandOptions)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = typeof(TCommand) });
        }

        /// <summary>
        /// Adds a command to the command options.
        /// </summary>
        /// <param name="commandOptions">The command options to configure.</param>
        /// <param name="commandType">The type of the command to add.</param>
        public static void AddCommand(this CommandOptions commandOptions, Type commandType)
        {
            commandOptions.CommandSources.Add(new ActualCommand { CommandType = commandType });
        }

        /// <summary>
        /// Adds a command assembly to the command options.
        /// </summary>
        /// <param name="commandOptions">The command options to configure.</param>
        /// <param name="commandAssembly">The assembly containing commands.</param>
        public static void AddCommandAssembly(this CommandOptions commandOptions, Assembly commandAssembly)
        {
            commandOptions.CommandSources.Add(new ActualCommandAssembly { Assembly = commandAssembly });
        }

        /// <summary>
        /// Adds a global command filter to the command options.
        /// </summary>
        /// <typeparam name="TCommandFilter">The type of the command filter to add.</typeparam>
        /// <param name="commandOptions">The command options to configure.</param>
        public static void AddGlobalCommandFilter<TCommandFilter>(this CommandOptions commandOptions)
            where TCommandFilter : CommandFilterBaseAttribute
        {
            commandOptions.AddGlobalCommandFilterType(typeof(TCommandFilter));
        }

        /// <summary>
        /// Adds a global command filter to the command options.
        /// </summary>
        /// <param name="commandOptions">The command options to configure.</param>
        /// <param name="commandFilterType">The type of the command filter to add.</param>
        /// <exception cref="Exception">Thrown if the command filter type does not inherit from <see cref="CommandFilterBaseAttribute"/>.</exception>
        public static void AddGlobalCommandFilter(this CommandOptions commandOptions, Type commandFilterType)
        {
            if (!typeof(CommandFilterBaseAttribute).IsAssignableFrom(commandFilterType))
                throw new Exception("The command filter type must inherit CommandFilterBaseAttribute.");

            commandOptions.AddGlobalCommandFilterType(commandFilterType);
        }
    }
}

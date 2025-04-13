using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    /// <summary>
    /// Defines a source for retrieving command types based on specified criteria.
    /// </summary>
    public interface ICommandSource
    {
        /// <summary>
        /// Retrieves a collection of command types that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria to filter command types.</param>
        /// <returns>An enumerable collection of command types that match the criteria.</returns>
        IEnumerable<Type> GetCommandTypes(Predicate<Type> criteria);
    }
}

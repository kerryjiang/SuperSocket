using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The pipeline data processing result
    /// </summary>
    public struct ProcessResult
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public ProcessState State { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; private set; }


        /// <summary>
        /// the all packages which are resolved by this round processing
        /// </summary>
        public IList<IPackageInfo> Packages { get; private set; }

        /// <summary>
        /// Creates a processing result with the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public static ProcessResult Create(ProcessState state)
        {
            var result = new ProcessResult();
            result.State = state;
            return result;
        }

        /// <summary>
        /// Creates a processing result with the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static ProcessResult Create(ProcessState state, string message)
        {
            return Create(state, message, null);
        }

        /// <summary>
        /// Creates a processing result with the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="packages">The packages which were processed in this round.</param>
        /// <returns></returns>
        public static ProcessResult Create(ProcessState state, IList<IPackageInfo> packages)
        {
            return Create(state, string.Empty, packages);
        }

        /// <summary>
        /// Creates a processing result with the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        /// <param name="packages">The packages which were processed in this round.</param>
        /// <returns></returns>
        public static ProcessResult Create(ProcessState state, string message, IList<IPackageInfo> packages)
        {
            var result = new ProcessResult();
            result.State = state;
            result.Message = message;
            result.Packages = packages;
            return result;
        }
    }
}

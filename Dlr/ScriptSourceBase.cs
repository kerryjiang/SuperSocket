using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Dlr
{
    /// <summary>
    /// ScriptSourceBase
    /// </summary>
    public abstract class ScriptSourceBase : IScriptSource
    {
        /// <summary>
        /// Gets the name matches with the command anme.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the language extension.
        /// </summary>
        public string LanguageExtension { get; protected set; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public abstract string Tag { get; }

        /// <summary>
        /// Gets the script code.
        /// </summary>
        /// <returns></returns>
        public abstract string GetScriptCode();

        /// <summary>
        /// Gets the last updated time.
        /// </summary>
        public DateTime LastUpdatedTime { get; protected set; }
    }
}

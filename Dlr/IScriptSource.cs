using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Dlr
{
    /// <summary>
    /// DynamicCommandSource interface
    /// </summary>
    public interface IScriptSource
    {
        /// <summary>
        /// Gets the name matches with the command anme.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the language extension.
        /// </summary>
        string LanguageExtension { get; }

        /// <summary>
        /// Gets the tag, a string can identify the script source.
        /// </summary>
        string Tag { get; }
        /// <summary>
        /// Gets the script code.
        /// </summary>
        /// <returns></returns>
        string GetScriptCode();

        /// <summary>
        /// Gets the last updated time.
        /// </summary>
        DateTime LastUpdatedTime { get;  }
    }
}

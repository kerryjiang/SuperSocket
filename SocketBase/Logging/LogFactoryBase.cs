using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SuperSocket.SocketBase.Logging
{
    /// <summary>
    /// LogFactory Base class
    /// </summary>
    public abstract class LogFactoryBase : ILogFactory
    {
        /// <summary>
        /// Gets the config file file path.
        /// </summary>
        protected string ConfigFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the server instance is running in isolation mode and the multiple server instances share the same logging configuration.
        /// </summary>
        protected bool IsSharedConfig { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactoryBase"/> class.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        protected LogFactoryBase(string configFile)
        {
            if (Path.IsPathRooted(configFile))
            {
                ConfigFile = configFile;
                return;
            }

            if (Path.DirectorySeparatorChar != '\\')
            {
                configFile = Path.GetFileNameWithoutExtension(configFile) + ".unix" + Path.GetExtension(configFile);
            }

            var currentAppDomain = AppDomain.CurrentDomain;
            var isolation = IsolationMode.None;

            var isolationValue = currentAppDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue != null)
                isolation = (IsolationMode)isolationValue;

            if (isolation == IsolationMode.None)
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                filePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                ConfigFile = configFile;
                return;
            }
            else //The running AppServer is in isolated appdomain
            {
                //1. search the appDomain's base directory
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                //go to the application's root
                //the appdomain's root is /WorkingDir/DomainName, so get parent path twice to reach the application root
                var rootDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

                //2. search the file with appdomain's name as prefix in the application's root
                //the config file whose name have appDomain's name has higher priority
                filePath = Path.Combine(rootDir, AppDomain.CurrentDomain.FriendlyName + "." + configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                //3. search in the application's root without appdomain's name as prefix
                filePath = Path.Combine(rootDir, configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    IsSharedConfig = true;
                    return;
                }

                
                rootDir = Path.Combine(rootDir, "Config");
                //Search the config file with appdomain's name as prefix in the Config dir
                filePath = Path.Combine(rootDir, AppDomain.CurrentDomain.FriendlyName + "." + configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    return;
                }

                filePath = Path.Combine(rootDir, configFile);

                if (File.Exists(filePath))
                {
                    ConfigFile = filePath;
                    IsSharedConfig = true;
                    return;
                }

                ConfigFile = configFile;
                return;
            }
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public abstract ILog GetLog(string name);
    }
}

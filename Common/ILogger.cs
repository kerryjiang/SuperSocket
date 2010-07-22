using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common
{
    public interface ILogger
    {
        void LogError(Exception e);

        void LogError(ILogApp app, Exception e);

        void LogError(string title, Exception e);

        void LogError(ILogApp app, string title, Exception e);

        void LogError(string message);

        void LogError(ILogApp app, string message);

        void LogDebug(string message);

        void LogDebug(ILogApp app, string message);

        void LogInfo(string message);

        void LogInfo(ILogApp app, string message);

        void LogPerf(string message);

        void LogPerf(ILogApp app, string message);
    }

    public interface ILogApp
    {
        string Name { get; }
    }
}

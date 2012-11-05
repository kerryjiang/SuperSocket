using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Logging;
using System.Threading;
using System.Reflection;
using System.IO;
using SuperSocket.SocketBase;

namespace SuperSocket.Test
{
    public static class Setup
    {
        public static void LoadConfigFileFromResource(string filePath)
        {
            var currentAssembly = typeof(Setup).Assembly;
            var resourceName = currentAssembly.GetName().Name + "." + filePath.Replace(Path.DirectorySeparatorChar, '.');
            using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                
                var dir = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(filePath, buffer);
            }
        }

        public static string GetResourceContent(string fileName)
        {
            var currentAssembly = typeof(Setup).Assembly;
            var resourceName = currentAssembly.GetName().Name + "." + fileName.Replace(Path.DirectorySeparatorChar, '.');

            using (var reader = new StreamReader(currentAssembly.GetManifestResourceStream(resourceName), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

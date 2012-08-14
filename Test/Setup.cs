using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Logging;
using System.Threading;
using System.Reflection;
using System.IO;

namespace SuperSocket.Test
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void RunBeforeAllTest()
        {
            Console.WriteLine("RunBeforeAllTest");

            //Extract all resource
            var currentAssembly = typeof(Setup).Assembly;
            var assemblyName = currentAssembly.GetName().Name;
            var names = currentAssembly.GetManifestResourceNames();

            foreach (var name in names)
            {
                var fileName = name.Substring(assemblyName.Length + 1);
                var fileNameSegments = fileName.Split('.');
                var filePath = string.Join(Path.DirectorySeparatorChar.ToString(), fileNameSegments, 0, fileNameSegments.Length -1) + "." + fileNameSegments[fileNameSegments.Length - 1];

                if(File.Exists(filePath))
                    continue;

                var dir = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using(var stream  = currentAssembly.GetManifestResourceStream(name))
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    File.WriteAllBytes(filePath, buffer);
                }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Security;
using System.IO;

namespace CertificateCreator
{
    class Program
    {
        internal static void CreateCertificate(string savePath, string commonName, string password)
        {
            if (!commonName.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                commonName = "CN=" + commonName;

            byte[] certificateData = Certificate.CreateSelfSignCertificatePfx(commonName, //host name
                DateTime.Now, //not valid before
                DateTime.Now.AddYears(5), //not valid after
                password);

            using (BinaryWriter binWriter = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                binWriter.Write(certificateData);
                binWriter.Flush();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter Common Name:");
            var commonName = Console.ReadLine();
            Console.WriteLine("Please enter Password:");
            var password = Console.ReadLine();
            Console.WriteLine("Please enter Save Path:");
            var savePath = Console.ReadLine();

            CreateCertificate(savePath, commonName, password);
            Console.WriteLine("CreateCertificate successfull, press any key to quit!");
            Console.ReadKey();
        }
    }
}

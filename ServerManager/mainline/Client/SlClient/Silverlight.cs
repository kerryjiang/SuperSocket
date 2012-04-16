using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SuperSocket.Management.Client
{
    public static class Silverlight
    {
        public static string GetSHA1Hash(string val)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(val);
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1Managed();
            byte[] res = sha.ComputeHash(data);
            return Convert.ToBase64String(res);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace SuperSocket.Management.Client
{
    public static class Extensions
    {
        public static void XmlSerialize(this object target, string savePath)
        {
            var serializer = new XmlSerializer(target.GetType());

            using (StreamWriter writer = new StreamWriter(savePath, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, target);
                writer.Close();
            }
        }

        public static T XmlDeserialize<T>(this string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                var target = serializer.Deserialize(reader);

                if(target == null)
                    return default(T);

                return (T)target;
            }
        }
    }
}

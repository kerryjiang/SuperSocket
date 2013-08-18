using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace SuperSocket.ServerManager.Client
{
    public static class Extensions
    {
        public static string XmlSerialize(this object target)
        {
            var serializer = new XmlSerializer(target.GetType());

            var stringBuilder = new StringBuilder();

            using (var writer = new StringWriter(stringBuilder))
            {
                serializer.Serialize(writer, target);
                writer.Close();
            }

            return stringBuilder.ToString();
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

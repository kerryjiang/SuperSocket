using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using SuperSocket.SocketBase;

namespace SuperSocket.Management.Server
{
    class ListenersJsonConverter : JsonConverter
    {
        private static Type m_ListenersType = typeof(ListenerInfo[]);

        public override bool CanConvert(Type objectType)
        {
            return objectType == m_ListenersType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            writer.WriteValue(string.Join(", ", ((ListenerInfo[])value).Select(l => l.EndPoint.ToString()).ToArray()));
        }
    }
}

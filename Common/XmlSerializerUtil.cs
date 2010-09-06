using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace SuperSocket.Common
{
    public static class XmlSerializerUtil
    {
        public static bool TryDeserialize<T>(string filePath, out T result)
        {
            result = default(T);

            XmlSerializer worker = new XmlSerializer(typeof(T));
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                result = (T)worker.Deserialize(stream);

                if (result == null)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        public static bool Serialize(string filePath, object target)
        {
            XmlSerializer worker = new XmlSerializer(target.GetType());
            Stream stream = null;
            XmlWriter writer = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write);
                writer = XmlWriter.Create(stream, GetWriterSetting());
                worker.Serialize(writer, target);
                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        private static XmlWriterSettings GetWriterSetting()
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Encoding = Encoding.UTF8;
            setting.Indent = true;
            return setting;
        }
    }
}

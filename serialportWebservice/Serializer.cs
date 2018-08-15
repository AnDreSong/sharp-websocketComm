using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace serialportWebservice
{
    public class Serializer
    {
        private static HybridDictionary serializers = new HybridDictionary();

        private static XmlSerializer CreateXmlSerializer(Type type)
        {
            string key = type.FullName;
            XmlSerializer x;
            if (serializers.Contains(key))
                x = serializers[key] as XmlSerializer;
            else
            {
                x = new XmlSerializer(type);
                lock (serializers.SyncRoot)
                {
                    serializers.Add(key, x);
                }
            }
            return x;
        }

        private static XmlSerializer CreateXmlSerializer(Type type, Type[] extraTypes)
        {
            string key = type.FullName;
            XmlSerializer x;
            if (serializers.Contains(key))
                x = serializers[key] as XmlSerializer;
            else
            {
                x = new XmlSerializer(type, extraTypes);
                lock (serializers.SyncRoot)
                {
                    serializers.Add(key, x);
                }
            }
            return x;
        }

        #region XML

        public static void ToXml(object graph, Stream stream)
        {
            XmlSerializer xml = CreateXmlSerializer(graph.GetType());
            xml.Serialize(stream, graph);
        }

        public static void ToXml(object graph, Stream stream, Type[] extraTypes)
        {
            XmlSerializer xml = CreateXmlSerializer(graph.GetType(), extraTypes);
            xml.Serialize(stream, graph);
        }

        public static void ToXml(object graph, string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                ToXml(graph, fs);
            }
        }

        public static void ToXml(object graph, string fileName, Type[] extraTypes)
        {
            string path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                ToXml(graph, fs, extraTypes);
            }
        }

        public static object FromXml(Stream stream, Type type)
        {
            XmlSerializer xml = CreateXmlSerializer(type);
            return xml.Deserialize(stream);
        }

        public static object FromXml(Stream stream, Type type, Type[] extraTypes)
        {
            XmlSerializer xml = CreateXmlSerializer(type, extraTypes);
            return xml.Deserialize(stream);
        }

        public static T FromXml<T>(Stream stream)
        {
            XmlSerializer xml = CreateXmlSerializer(typeof(T));
            return (T)xml.Deserialize(stream);
        }

        public static T FromXml<T>(Stream stream, Type[] extraTypes)
        {
            XmlSerializer xml = CreateXmlSerializer(typeof(T), extraTypes);
            return (T)xml.Deserialize(stream);
        }

        public static T FromXml<T>(string fileName)
        {
            if (!File.Exists(fileName))
                throw new ArgumentNullException(string.Format("文件【{0}】不存在", fileName));
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return (T)FromXml(fs, typeof(T));
            }
        }

        public static T FromXml<T>(string fileName, Type[] extraTypes)
        {
            if (!File.Exists(fileName))
                throw new ArgumentNullException(string.Format("文件【{0}】不存在", fileName));
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return (T)FromXml(fs, typeof(T), extraTypes);
            }
        }

        #endregion XML

        public static void ToBIN(object graph, Stream stream, bool compressed = true)
        {
            IFormatter format = new BinaryFormatter();
            if (!compressed)
            {
                format.Serialize(stream, graph);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    format.Serialize(ms, graph);
                    byte[] source = ms.ToArray();
                    using (GZipStream gs = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        gs.Write(source, 0, source.Length);
                    }
                }
            }
        }

        public static object FromBIN(Stream stream, bool compressed = true)
        {
            IFormatter format = new BinaryFormatter();
            if (!compressed)
            {
                return format.Deserialize(stream);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        byte[] buff = new byte[BuffSize];
                        int reads = 0;

                        while ((reads = zip.Read(buff, 0, BuffSize)) > 0)
                            ms.Write(buff, 0, reads);

                        ms.Position = 0;
                        return format.Deserialize(ms);
                    }
                }
            }
        }

        public static T FromBIN<T>(Stream stream, bool compressed = true)
        {
            IFormatter format = new BinaryFormatter();
            if (!compressed)
            {
                return (T)format.Deserialize(stream);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        byte[] buff = new byte[BuffSize];
                        int reads = 0;

                        while ((reads = zip.Read(buff, 0, BuffSize)) > 0)
                            ms.Write(buff, 0, reads);

                        ms.Position = 0;
                        return (T)format.Deserialize(ms);
                    }
                }
            }
        }

        private const int BuffSize = 8192;
    }
}
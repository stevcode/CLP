using System.IO;
using System.Xml;
using System.Xml.Linq;
using Catel.Data;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Xml;

namespace CLP.Entities
{
    public abstract class ASerializableBase : ModelBase
    {
        public string ToXmlString(XmlSerializerOptimalizationMode optimalizationMode = XmlSerializerOptimalizationMode.Performance)
        {
            using (var memoryStream = new MemoryStream())
            {
                var configuration = new XmlSerializationConfiguration
                                    {
                                        OptimalizationMode = optimalizationMode
                                    };

                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, memoryStream, configuration);

                memoryStream.Position = 0L;
                using (var xmlReader = XmlReader.Create(memoryStream))
                {
                    return XDocument.Load(xmlReader).ToString();
                }
            }
        }

        public void ToXmlFile(string filePath, XmlSerializerOptimalizationMode optimalizationMode = XmlSerializerOptimalizationMode.Performance)
        {
            // Copied from personal json implementation
            var xmlString = ToXmlString(optimalizationMode);
            File.WriteAllText(filePath, xmlString);

            // From Catel Example
            //var fileInfo = new FileInfo(filePath);
            //if (!Directory.Exists(fileInfo.DirectoryName))
            //{
            //    Directory.CreateDirectory(fileInfo.DirectoryName);
            //}

            //using (Stream stream = new FileStream(filePath, FileMode.Create))
            //{
            //    var configuration = new XmlSerializationConfiguration
            //                        {
            //                            OptimalizationMode = optimalizationMode
            //                        };

            //    var xmlSerializer = SerializationFactory.GetXmlSerializer();
            //    xmlSerializer.Serialize(this, stream, configuration);

            //    ClearIsDirtyOnAllChilds();
            //}
        }

        public static T FromXmlString<T>(string xml) where T : class
        {
            var xmlDocument = XDocument.Parse(xml);

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream))
                {
                    xmlDocument.Save(xmlWriter);
                }

                memoryStream.Position = 0L;

                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                var deserialized = xmlSerializer.Deserialize(typeof(T), memoryStream, null);
                return (T) deserialized;
            }
        }

        public static T FromXmlFile<T>(string filePath) where T : class
        {
            var xmlString = File.ReadAllText(filePath);
            return FromXmlString<T>(xmlString);
        }
    }
}
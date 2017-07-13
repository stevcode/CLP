using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Json;
using Catel.Runtime.Serialization.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using JsonSerializer = Catel.Runtime.Serialization.Json.JsonSerializer;

namespace CLP.Entities
{
    public abstract class AEntityBase : ModelBase
    {
        private static readonly ISerializationManager SerializationManager = ServiceLocator.Default.ResolveType<ISerializationManager>();
        private static readonly IObjectAdapter ObjectAdapter = ServiceLocator.Default.ResolveType<IObjectAdapter>();

        protected AEntityBase() { }

        protected AEntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string ToJsonString(bool formatWithIndents = true)
        {
            using (var memoryStream = new MemoryStream())
            {
                var configuration = new JsonSerializationConfiguration
                                    {
                                        Formatting = formatWithIndents ? Formatting.Indented : Formatting.None,
                                        IsEnumSerializedWithString = true,
                                        DateParseHandling = DateParseHandling.DateTime,
                                        DateTimeKind = DateTimeKind.Unspecified,
                                        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
                                    };

                var jsonSerializer = new JsonSerializer(SerializationManager, TypeFactory.Default, ObjectAdapter);
                jsonSerializer.Serialize(this, memoryStream, configuration);
                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream))
                {
                    var jsonString = reader.ReadToEnd();
                    return jsonString;
                }
            }
        }

        public void ToJsonFile(string filePath, bool formatWithIndents = true)
        {
            var jsonString = ToJsonString(formatWithIndents);
            File.WriteAllText(filePath, jsonString);
        }

        public static T FromJsonString<T>(string json) where T : class
        {
            using (var memoryStream = new MemoryStream(Encoding.Default.GetBytes(json)))
            {
                var configuration = new JsonSerializationConfiguration
                                    {
                                        IsEnumSerializedWithString = true,
                                        DateParseHandling = DateParseHandling.DateTime,
                                        DateTimeKind = DateTimeKind.Unspecified,
                                        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
                                    };

                var jsonSerializer = new JsonSerializer(SerializationManager, TypeFactory.Default, ObjectAdapter);
                //try
                //{
                    var deserialized = jsonSerializer.Deserialize(typeof(T), memoryStream, configuration);
                    return (T)deserialized;
                //}
                //catch (Exception ex)
                //{
                //    CLogger.AppendToLog($"Error trying to deserialize {typeof(T)} via json.\n{ex.Message}");
                //    return null;
                //}
            }
        }

        public static T FromJsonFile<T>(string filePath) where T : class
        {
            var json = File.ReadAllText(filePath);
            return FromJsonString<T>(json);
        }

        public string ToXmlString()
        {
            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, memoryStream, null);

                memoryStream.Position = 0L;
                using (var xmlReader = XmlReader.Create(memoryStream))
                {
                    return XDocument.Load(xmlReader).ToString();
                }
            }
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
                return (T) xmlSerializer.Deserialize(typeof(T), memoryStream, null);
            }
        }
    }
}
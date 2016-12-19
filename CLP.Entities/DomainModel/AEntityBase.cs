using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Json;
using Newtonsoft.Json;
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
            using (var stream = new MemoryStream())
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
                jsonSerializer.Serialize(this, stream, configuration);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var jsonString = reader.ReadToEnd();
                    return jsonString;
                }
            }
        }

        public static T FromJsonString<T>(string json) where T : class
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(json)))
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
                    var deserialized = jsonSerializer.Deserialize(typeof(T), stream, configuration);
                    return (T)deserialized;
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Error trying to deserialize {typeof(T)} via json.\n{ex.Message}");
                //    return null;
                //}
            }
        }
    }
}
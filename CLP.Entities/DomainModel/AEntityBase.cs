using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization.Json;

namespace CLP.Entities
{
    public abstract class AEntityBase : ModelBase
    {
        protected AEntityBase() { }

        protected AEntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public void ClearDirtyFlag() { IsDirty = false; }

        public string ToJsonString()
        {
            using (var stream = new MemoryStream())
            {
                var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
                jsonSerializer.WriteTypeInfo = true;
                jsonSerializer.PreserveReferences = true;
                jsonSerializer.Serialize(this, stream);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var jsonString = reader.ReadToEnd();
                return jsonString;
            }
        }

        public static T Load<T>(string fileName, SerializationMode mode)
             where T : class
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open))
            {
                return Load<T>(stream, mode);
            }
        }

        public static T FromJsonString<T>(string json)
             where T : class
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(json)))
            {
                var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
                var deserialized = jsonSerializer.Deserialize(typeof(T), stream);
                return (T)deserialized;
            }
        }
    }
}
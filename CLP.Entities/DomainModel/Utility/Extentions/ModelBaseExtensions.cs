using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Json;

namespace CLP.Entities
{
    public static class ModelBaseExtensions
    {
        public static T DeepCopy<T>(this T model) where T : ModelBase
        {
            if (!model.GetType().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(model));
            }

            // Don't serialize a null object
            if (ReferenceEquals(model, null))
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = SerializationFactory.GetXmlSerializer();
                    serializer.Serialize(model, stream);
                    stream.Position = 0L;
                    var clone = serializer.Deserialize(model.GetType(), stream); //model.GetType() or typeof(T)?
                    return clone as T;
                }

                // BUG: Potential bug, see if formatter can ignore anything marked as "Ignore Serialization"
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, model);
                    stream.Position = 0L;
                    var clone = formatter.Deserialize(stream);
                    return clone as T;
                }

                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, model);
                    stream.Position = 0L;
                    var clone = formatter.Deserialize(stream);
                    return clone as T;
                }

                using (var stream = new MemoryStream())
                {
                    var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
                    jsonSerializer.WriteTypeInfo = true;
                    jsonSerializer.PreserveReferences = true;
                    jsonSerializer.Serialize(model, stream);
                    stream.Position = 0;
                    var clone = jsonSerializer.Deserialize(typeof(T), stream);
                    return clone as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
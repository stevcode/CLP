using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Catel.Data;

namespace CLP.Entities
{
    public static class ModelBaseExtensions
    {
        public static T DeepCopy<T>(this T model) where T : ModelBase
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "model");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(model, null))
            {
                return default(T);
            }

            // BUG: Potential bug, see if formatter can ignore anything marked as "Ignore Serialization"
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, model);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
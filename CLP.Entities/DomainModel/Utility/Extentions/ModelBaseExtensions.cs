using System;
using System.IO;
using Catel;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public static class ModelBaseExtensions
    {
        public static T DeepCopy<T>(this T model) where T : ModelBase
        {
            Argument.IsNotNull(() => model);

            if (!model.GetType().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(model));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    // TODO: Test speed with SerializationFactory.GetBinarySerializer();
                    // BUG: Potential bug, see if formatter can ignore anything marked as "Ignore Serialization"
                    var serializer = SerializationFactory.GetXmlSerializer(); 
                    serializer.Serialize(model, stream, null);
                    stream.Position = 0L;
                    var clone = serializer.Deserialize(model.GetType(), stream, null);
                    return clone as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
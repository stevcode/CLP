using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization.Json;

namespace CLP.Entities
{
    public static class ModelBaseExtensions
    {
        //public static T DeepCopy<T>(this T model) where T : ModelBase
        //{
        //    if (!model.GetType().IsSerializable)
        //    {
        //        throw new ArgumentException("The type must be serializable.", nameof(model));
        //    }

        //    // Don't serialize a null object, simply return the default for that object
        //    if (ReferenceEquals(model, null))
        //    {
        //        return default(T);
        //    }

        //    return AEntityBase.FromJsonString<T>()

        //    // BUG: Potential bug, see if formatter can ignore anything marked as "Ignore Serialization"
        //    IFormatter formatter = new BinaryFormatter();
        //    Stream stream = new MemoryStream();
        //    using (stream)
        //    {
        //        formatter.Serialize(stream, model);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return (T)formatter.Deserialize(stream);
        //    }
        //}

        //public static string ToJsonString(this ModelBase model, bool formatWithIndents = false)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
        //        jsonSerializer.WriteTypeInfo = true;
        //        jsonSerializer.PreserveReferences = true;
        //        jsonSerializer.FormatWithIndents = formatWithIndents;
        //        jsonSerializer.Serialize(model, stream);
        //        stream.Position = 0;
        //        var reader = new StreamReader(stream);
        //        var jsonString = reader.ReadToEnd();
        //        return jsonString;
        //    }
        //}
    }
}
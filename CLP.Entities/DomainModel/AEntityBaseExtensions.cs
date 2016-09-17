using System;

namespace CLP.Entities
{
    public static class AEntityBaseExtensions
    {
        //public static T DeepCopy<T>(this T model) where T : AEntityBase
        //{
        //    var modelType = model.GetType();

        //    if (!modelType.IsSerializable)
        //    {
        //        throw new ArgumentException("The type must be serializable.", modelType.ToString());
        //    }

        //    // Don't serialize a null object, simply return the default for that object
        //    return ReferenceEquals(model, null) ? default(T) : AEntityBase.FromJsonString<>(model.ToJsonString(true));
        //}
    }
}
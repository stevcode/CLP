using System.IO;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    public abstract class AEntityBase : ModelBase
    {
        protected AEntityBase() { }

        protected AEntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public void ClearDirtyFlag() { IsDirty = false; }

        public static T Load<T>(string fileName, SerializationMode mode)
             where T : class
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open))
            {
                return Load<T>(stream, mode);
            }
        }
    }
}
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class EntityBase : ModelBase
    {
        protected EntityBase() { }

        protected EntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public void ClearDirtyFlag() { IsDirty = false; }
    }
}
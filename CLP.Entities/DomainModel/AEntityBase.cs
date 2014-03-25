using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class AEntityBase : ModelBase
    {
        protected AEntityBase() { }

        protected AEntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public void ClearDirtyFlag() { IsDirty = false; }
    }
}
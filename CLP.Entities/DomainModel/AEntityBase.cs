using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [RedirectType("CLP.Entities", "AEntityBase")]
    public abstract class AEntityBase : ModelBase
    {
        protected AEntityBase() { }

        protected AEntityBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public void ClearDirtyFlag() { IsDirty = false; }
    }
}
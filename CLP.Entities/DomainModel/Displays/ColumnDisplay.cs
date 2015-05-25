using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ColumnDisplay : ADisplayBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="GridDisplay" /> from scratch.</summary>
        public ColumnDisplay() { }

        /// <summary>Initializes <see cref="GridDisplay" /> from parent <see cref="Notebook" />.</summary>
        public ColumnDisplay(Notebook notebook)
            : base(notebook) { }

        /// <summary>Initializes <see cref="GridDisplay" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ColumnDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}
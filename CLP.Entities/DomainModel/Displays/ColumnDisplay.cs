using System;

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

        #endregion //Constructors
    }
}
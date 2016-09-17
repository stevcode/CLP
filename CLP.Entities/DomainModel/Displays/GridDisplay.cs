using System;

namespace CLP.Entities
{
    [Serializable]
    public class GridDisplay : ADisplayBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="GridDisplay" /> from scratch.</summary>
        public GridDisplay() { }

        /// <summary>Initializes <see cref="GridDisplay" /> from parent <see cref="Notebook" />.</summary>
        public GridDisplay(Notebook notebook)
            : base(notebook) { }

        #endregion //Constructors
    }
}
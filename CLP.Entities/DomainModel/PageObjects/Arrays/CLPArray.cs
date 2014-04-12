using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class CLPArray : ACLPArrayBase, ICountable
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArray" /> from scratch.
        /// </summary>
        public CLPArray() { }

        /// <summary>
        /// Initializes <see cref="CLPArray" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="CLPArray" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        public CLPArray(CLPPage parentPage, int columns, int rows)
            : base(parentPage, columns, rows) { }

        /// <summary>
        /// Initializes <see cref="CLPArray" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newCLPArray = Clone() as CLPArray;
            if(newCLPArray == null)
            {
                return null;
            }
            newCLPArray.CreationDate = DateTime.Now;
            newCLPArray.ID = Guid.NewGuid().ToString();
            newCLPArray.VersionIndex = 0;
            newCLPArray.LastVersionIndex = null;
            newCLPArray.ParentPage = ParentPage;

            return newCLPArray;
        }

        #endregion //Methods

        #region Implementation of ICountable

        /// <summary>
        /// Number of Parts the <see cref="ICountable" /> represents.
        /// </summary>
        public int Parts
        {
            get { return Rows * Columns; }
            set { }
        }

        /// <summary>
        /// Signifies the <see cref="ICountable" /> has been accepted by another <see cref="ICountable" />.
        /// </summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof(bool), false);

        #endregion
    }
}
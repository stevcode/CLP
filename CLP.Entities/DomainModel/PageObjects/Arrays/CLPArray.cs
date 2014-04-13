using System;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum ArrayTypes
    {
        Array,
        ArrayCard,
        FactorCard
    }
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
        public CLPArray(CLPPage parentPage, int columns, int rows, ArrayTypes arrayType)
            : base(parentPage, columns, rows)
        {
            IsGridOn = rows < 50 && columns < 50;
            ArrayType = arrayType;
        }

        /// <summary>
        /// Initializes <see cref="CLPArray" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        private const double MIN_ARRAY_LENGTH = 25.0;

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public override double MinimumHeight
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override double MinimumWidth
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override double MinimumGridSquareSize
        {
            get { return Columns < Rows ? MIN_ARRAY_LENGTH / Columns : MIN_ARRAY_LENGTH / Rows; }
        }

        /// <summary>
        /// The type of <see cref="CLPArray" />.
        /// </summary>
        public ArrayTypes ArrayType
        {
            get { return GetValue<ArrayTypes>(ArrayTypeProperty); }
            set { SetValue(ArrayTypeProperty, value); }
        }

        public static readonly PropertyData ArrayTypeProperty = RegisterProperty("ArrayType", typeof(ArrayTypes), ArrayTypes.Array);

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

        #region Overrides of APageObjectBase

        public override void OnAdded()
        {
            base.OnAdded();
            // If FFC with remainder on page, update
            foreach(var ffc in ParentPage.PageObjects.OfType<FuzzyFactorCard>()) 
            {
                ffc.AnalyzeArrays();
                ffc.UpdateRemainderRegion();
            }
        }

        public override void OnDeleted()
        {
            base.OnDeleted();
            // If FFC with remainder on page, update
            foreach(var ffc in ParentPage.PageObjects.OfType<FuzzyFactorCard>()) 
            {
                ffc.AnalyzeArrays();
                ffc.UpdateRemainderRegion();
            }
        }

        #endregion //Overrides of APageObjectBase

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var initialSquareSize = 45.0;
            if(toSquareSize <= 0)
            {
                while(XPosition + 2 * LabelLength + initialSquareSize * Columns >= ParentPage.Width ||
                      YPosition + 2 * LabelLength + initialSquareSize * Rows >= ParentPage.Height)
                {
                    initialSquareSize = Math.Abs(initialSquareSize - 45.0) < .0001 ? 22.5 : initialSquareSize / 4 * 3;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            Height = (initialSquareSize * Rows) + (2 * LabelLength);
            Width = (initialSquareSize * Columns) + (2 * LabelLength);

            if(recalculateDivisions)
            {
                //ResizeDivisions();
            }
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
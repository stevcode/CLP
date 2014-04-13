using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class ACLPArrayBase : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> from scratch.
        /// </summary>
        public ACLPArrayBase() { }

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ACLPArrayBase" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        public ACLPArrayBase(CLPPage parentPage, int columns, int rows)
            : base(parentPage)
        {
            Columns = columns;
            Rows = rows;
        }

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ACLPArrayBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public virtual double LabelLength
        {
            get { return 22; }
        }

        /// <summary>
        /// The number of rows in the <see cref="ACLPArrayBase" />.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 1);

        /// <summary>
        /// The number of columns in the <see cref="ACLPArrayBase" />.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 1);

        #region Behavior Properties

        /// <summary>
        /// Turns the grid on or off.
        /// </summary>
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(bool), true);

        /// <summary>
        /// Turns the division behavior on or off.
        /// </summary>
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(bool), false);

        /// <summary>
        /// Whether the array can be snapped to other arrays or not.
        /// </summary>
        public bool IsSnappable
        {
            get { return GetValue<bool>(IsSnappableProperty); }
            set { SetValue(IsSnappableProperty, value); }
        }

        public static readonly PropertyData IsSnappableProperty = RegisterProperty("IsSnappable", typeof(bool), true);

        /// <summary>
        /// Sets the visibility of the array's top label.
        /// </summary>
        public bool IsTopLabelVisible
        {
            get { return GetValue<bool>(IsTopLabelVisibleProperty); }
            set { SetValue(IsTopLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof(bool), true);

        /// <summary>
        /// Sets the visibility of the array's side label.
        /// </summary>
        public bool IsSideLabelVisible
        {
            get { return GetValue<bool>(IsSideLabelVisibleProperty); }
            set { SetValue(IsSideLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideLabelVisibleProperty = RegisterProperty("IsSideLabelVisible", typeof(bool), true);

        #endregion //Behavior Properties

        #region Calculated Properties

        public double ArrayWidth
        {
            get { return Width - (2 * LabelLength); }
        }

        public double ArrayHeight
        {
            get { return Height - (2 * LabelLength); }
        }

        public double GridSquareSize
        {
            get { return ArrayWidth / Columns; }
        }

        public abstract double MinimumGridSquareSize { get; }

        #endregion //Calculated Properties

        #endregion //Properties

        #region Methods

        public abstract void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true);

        #endregion //Methods

        #region Overrides of ObservableObject

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Height")
            {
                RaisePropertyChanged("ArrayHeight");
            }
            if(e.PropertyName == "Width")
            {
                RaisePropertyChanged("ArrayWidth");
                RaisePropertyChanged("GridSquareSize");
            }
            base.OnPropertyChanged(e);
        }

        #endregion //Overrides of ObservableObject

        #region Static Methods

        public static void ApplyDistinctPosition(ACLPArrayBase array, string currentUserID)
        {
            //squareSize will be the grid size of the most recently placed array, or 0 if there are no non-background arrays
            var lastArray =
                array.ParentPage.PageObjects.LastOrDefault(x => x is ACLPArrayBase && (x.OwnerID != Person.Author.ID || currentUserID == Person.Author.ID) && x.ID != array.ID) as ACLPArrayBase;
            if(lastArray == null)
            {
                return;
            }

            var previousGridSquareSize = array.GridSquareSize;
            array.SizeArrayToGridLevel(lastArray.GridSquareSize);
            if(lastArray is CLPArray)
            {
                array.YPosition = lastArray.YPosition;
                array.XPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Width;
                if(array.XPosition + array.Width >= array.ParentPage.Width ||
                   array.YPosition + array.Height >= array.ParentPage.Height)
                {
                    array.XPosition = lastArray.XPosition;
                    array.YPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Height;
                }
                if(array.XPosition + array.Width >= array.ParentPage.Width ||
                   array.YPosition + array.Height >= array.ParentPage.Height)
                {
                    ApplyDistinctPosition(array);
                }
            }
            else if(lastArray is FuzzyFactorCard)
            {
                array.XPosition = lastArray.XPosition;
                array.YPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Height + (lastArray as FuzzyFactorCard).LargeLabelLength - array.LabelLength;
                if(array.XPosition + array.Width >= array.ParentPage.Width ||
                   array.YPosition + array.Height >= array.ParentPage.Height)
                {
                    array.XPosition = 0.0;
                }
                if(array.XPosition + array.Width >= array.ParentPage.Width ||
                   array.YPosition + array.Height >= array.ParentPage.Height)
                {
                    ApplyDistinctPosition(array);
                }
            }
        }

        #endregion //Static Methods
    }
}
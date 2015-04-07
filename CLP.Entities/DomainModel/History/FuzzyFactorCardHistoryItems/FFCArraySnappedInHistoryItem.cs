using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FFCArraySnappedInHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="FFCArraySnappedInHistoryItem" /> from scratch.
        /// </summary>
        public FFCArraySnappedInHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="FFCArraySnappedInHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public FFCArraySnappedInHistoryItem(CLPPage parentPage, Person owner, string fuzzyFactorCardID, CLPArray snappedInArray)
            : base(parentPage, owner)
        {
            FuzzyFactorCardID = fuzzyFactorCardID;
            SnappedInArrayID = snappedInArray.ID;
            parentPage.History.TrashedPageObjects.Add(snappedInArray);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected FFCArraySnappedInHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        /// UniqueID of the FFC which had an array snapped inside
        /// </summary>
        public string FuzzyFactorCardID
        {
            get { return GetValue<string>(FuzzyFactorCardIDProperty); }
            set { SetValue(FuzzyFactorCardIDProperty, value); }
        }

        public static readonly PropertyData FuzzyFactorCardIDProperty = RegisterProperty("FuzzyFactorCardID", typeof(string), string.Empty);

        /// <summary>
        /// UniqueID of the array that wass snapped in and then deleted.
        /// </summary>
        public string SnappedInArrayID
        {
            get { return GetValue<string>(SnappedInArrayIDProperty); }
            set { SetValue(SnappedInArrayIDProperty, value); }
        }

        public static readonly PropertyData SnappedInArrayIDProperty = RegisterProperty("SnappedInArrayID", typeof(string), string.Empty);

        public override string FormattedValue
        {
            get
            {
                var snappedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(SnappedInArrayID) as CLPArray;
                var ffc = ParentPage.GetPageObjectByIDOnPageOrInHistory(FuzzyFactorCardID) as FuzzyFactorCard;
                string formattedValue = string.Format("Index # {0}, Snapped array({1} by {2}) into fuzzy factor card ({3} by {4}).", 
                    HistoryIndex, snappedArray.Rows, snappedArray.Columns, ffc.Rows, ffc.Columns);
                return formattedValue;
            }
        }
        
        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var fuzzyFactorCard = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            var snappedInArray = ParentPage.GetVerifiedPageObjectInTrashByID(SnappedInArrayID) as CLPArray;
            if(fuzzyFactorCard == null ||
               snappedInArray == null)
            {
                return;
            }
            
            ParentPage.History.TrashedPageObjects.Remove(snappedInArray);
            ParentPage.PageObjects.Add(snappedInArray);
            fuzzyFactorCard.RemoveLastDivision();
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var fuzzyFactorCard = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            var snappedInArray = ParentPage.GetVerifiedPageObjectOnPageByID(SnappedInArrayID) as CLPArray;
            if(fuzzyFactorCard == null ||
               snappedInArray == null)
            {
                return;
            }

            ParentPage.PageObjects.Remove(snappedInArray);
            ParentPage.History.TrashedPageObjects.Add(snappedInArray);
            fuzzyFactorCard.SnapInArray(snappedInArray.Columns);
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as FFCArraySnappedInHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}
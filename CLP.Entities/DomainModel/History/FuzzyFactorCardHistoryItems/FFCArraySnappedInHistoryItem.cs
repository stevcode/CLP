﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FFCArraySnappedInHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="FFCArraySnappedInHistoryItem" /> from scratch.</summary>
        public FFCArraySnappedInHistoryItem() { }

        /// <summary>Initializes <see cref="FFCArraySnappedInHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public FFCArraySnappedInHistoryItem(CLPPage parentPage, Person owner, string divisionTemplateID, CLPArray snappedInArray)
            : base(parentPage, owner)
        {
            FuzzyFactorCardID = divisionTemplateID;
            SnappedInArrayID = snappedInArray.ID;
            parentPage.History.TrashedPageObjects.Add(snappedInArray);
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
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

        /// <summary>UniqueID of the FFC which had an array snapped inside</summary>
        public string FuzzyFactorCardID
        {
            get { return GetValue<string>(FuzzyFactorCardIDProperty); }
            set { SetValue(FuzzyFactorCardIDProperty, value); }
        }

        public static readonly PropertyData FuzzyFactorCardIDProperty = RegisterProperty("FuzzyFactorCardID", typeof (string), string.Empty);

        /// <summary>UniqueID of the array that wass snapped in and then deleted.</summary>
        public string SnappedInArrayID
        {
            get { return GetValue<string>(SnappedInArrayIDProperty); }
            set { SetValue(SnappedInArrayIDProperty, value); }
        }

        public static readonly PropertyData SnappedInArrayIDProperty = RegisterProperty("SnappedInArrayID", typeof (string), string.Empty);

        public override string FormattedValue
        {
            get
            {
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(FuzzyFactorCardID) as FuzzyFactorCard;
                if (divisionTemplate == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Template for Array Snapped In not found on page or in history.", HistoryIndex);
                }

                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(SnappedInArrayID) as CLPArray;
                if (array == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Array for Array Snapped In not found on page or in history.", HistoryIndex);
                }

                return string.Format("Index #{0}, Snapped Array [{1}x{2}] into Division Template [{3}/{4}].",
                                     HistoryIndex,
                                     array.Rows,
                                     array.Columns,
                                     divisionTemplate.Dividend,
                                     divisionTemplate.Rows);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            UndoAction(false);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Array Snapped In not found on page or in history.", HistoryIndex);
                return;
            }

            var array = ParentPage.GetVerifiedPageObjectInTrashByID(SnappedInArrayID) as CLPArray;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Array Snapped In not found on page or in history.", HistoryIndex);
                return;
            }

            ParentPage.History.TrashedPageObjects.Remove(array);
            ParentPage.PageObjects.Add(array);
            divisionTemplate.RemoveLastDivision();
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Array Snapped In not found on page or in history.", HistoryIndex);
                return;
            }

            var array = ParentPage.GetVerifiedPageObjectOnPageByID(SnappedInArrayID) as CLPArray;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Array Snapped In not found on page or in history.", HistoryIndex);
                return;
            }

            ParentPage.PageObjects.Remove(array);
            ParentPage.History.TrashedPageObjects.Add(array);
            divisionTemplate.SnapInArray(array.Columns);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as FFCArraySnappedInHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return FuzzyFactorCardID == id || SnappedInArrayID == id; }

        #endregion //Methods
    }
}
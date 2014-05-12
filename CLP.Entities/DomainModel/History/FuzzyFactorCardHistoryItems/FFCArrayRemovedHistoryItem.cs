﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FFCArrayRemovedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="FFCArrayRemovedHistoryItem" /> from scratch.
        /// </summary>
        public FFCArrayRemovedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="FFCArrayRemovedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public FFCArrayRemovedHistoryItem(CLPPage parentPage, Person owner, string ffcUniqueID, int divisionValue)
            : base(parentPage, owner)
        {
            FFCUniqueID = ffcUniqueID;
            DivisionValue = divisionValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected FFCArrayRemovedHistoryItem(SerializationInfo info, StreamingContext context)
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
        public string FFCUniqueID
        {
            get { return GetValue<string>(FFCUniqueIDProperty); }
            set { SetValue(FFCUniqueIDProperty, value); }
        }

        public static readonly PropertyData FFCUniqueIDProperty = RegisterProperty("FFCUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Value of the label on the removed division.
        /// </summary>
        public int DivisionValue
        {
            get { return GetValue<int>(DivisionValueProperty); }
            set { SetValue(DivisionValueProperty, value); }
        }

        /// <summary>
        /// Register the value property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DivisionValueProperty = RegisterProperty("DivisionValue", typeof(int));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var ffc = ParentPage.GetPageObjectByID(FFCUniqueID) as FuzzyFactorCard;
            if(ffc != null)
            {
                ffc.SnapInArray(DivisionValue);
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var ffc = ParentPage.GetPageObjectByID(FFCUniqueID) as FuzzyFactorCard;
            if(ffc != null)
            {
                ffc.RemoveLastDivision();
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as FFCArrayRemovedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}
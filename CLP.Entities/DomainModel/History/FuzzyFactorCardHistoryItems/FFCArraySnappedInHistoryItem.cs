using System;
using System.Collections.Generic;
using System.Linq;
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
        public FFCArraySnappedInHistoryItem(CLPPage parentPage, Person owner, string ffcUniqueID, CLPArray snappedInArray)
            : base(parentPage, owner)
        {
            FFCUniqueID = ffcUniqueID;
            SnappedInArray = snappedInArray;
            SnappedInArrayUniqueID = snappedInArray.ID;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected FFCArraySnappedInHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 600;
            }
        }

        /// <summary>
        /// UniqueID of the FFC which had an array snapped inside
        /// </summary>
        public string FFCUniqueID
        {
            get
            {
                return GetValue<string>(FFCUniqueIDProperty);
            }
            set
            {
                SetValue(FFCUniqueIDProperty, value);
            }
        }

        public static readonly PropertyData FFCUniqueIDProperty = RegisterProperty("FFCUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Array which was snapped into the FFC. Null if it's currently on the page.
        /// </summary>
        public CLPArray SnappedInArray
        {
            get
            {
                return GetValue<CLPArray>(SnappedInArrayProperty);
            }
            set
            {
                SetValue(SnappedInArrayProperty, value);
            }
        }

        public static readonly PropertyData SnappedInArrayProperty = RegisterProperty("SnappedInArray", typeof(CLPArray));

        /// <summary>
        /// UniqueID of the array that wass snapped in and then deleted. 
        /// </summary>
        public string SnappedInArrayUniqueID
        {
            get
            {
                return GetValue<string>(SnappedInArrayUniqueIDProperty);
            }
            set
            {
                SetValue(SnappedInArrayUniqueIDProperty, value);
            }
        }

        public static readonly PropertyData SnappedInArrayUniqueIDProperty = RegisterProperty("SnappedInArrayUniqueID", typeof(string), string.Empty);

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
                SnappedInArray.ParentPage = ParentPage;
                ParentPage.PageObjects.Add(SnappedInArray);
                SnappedInArray = null;
                ffc.RemoveLastDivision();
            }
            else
            {
                //Logger.Instance.WriteToLog("Fuzzy Factor Card not found on page for UndoAction");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var array = ParentPage.GetPageObjectByID(SnappedInArrayUniqueID);
            if(array == null)
            {
                //Logger.Instance.WriteToLog("Array not found on page for RedoAction in HistoryFFCArraySnappedIn.");
                return;
            }
            SnappedInArray = array as CLPArray;
            var ffc = ParentPage.GetPageObjectByID(FFCUniqueID) as FuzzyFactorCard;
            if(ffc != null)
            {
                ParentPage.PageObjects.Remove(SnappedInArray);
                if(ffc.IsHorizontallyAligned)
                {
                    ffc.SnapInArray(SnappedInArray.Columns);
                }
                else
                {
                    ffc.SnapInArray(SnappedInArray.Rows);
                }
            }
            else
            {
                //Logger.Instance.WriteToLog("Fuzzy Factor Card not found on page for RedoAction");
            }
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
        public override void UnpackHistoryItem()
        {
        }


        #endregion //Methods
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayMassAddMassResize : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryArrayMassAddMassResize(ICLPPage parentPage, IEnumerable<string> pageObjectIDs, Dictionary<string, Point> oldDimensions)
            : base(parentPage)
        {
            PageObjectIDs = pageObjectIDs.ToList();
            OldDimensions = oldDimensions;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArrayMassAddMassResize(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 400;
            }
        }

        /// <summary>
        /// List of all the UniqueIDs of the pageObjects added.
        /// </summary>
        public List<string> PageObjectIDs
        {
            get
            {
                return GetValue<List<string>>(PageObjectIDsProperty);
            }
            set
            {
                SetValue(PageObjectIDsProperty, value);
            }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof(List<string>));

        /// <summary>
        /// List of the pageObjects that were removed from the page as a result of the UndoAction(). Cleared on Redo().
        /// </summary>
        public List<ICLPPageObject> PageObjects
        {
            get
            {
                return GetValue<List<ICLPPageObject>>(PageObjectsProperty);
            }
            set
            {
                SetValue(PageObjectsProperty, value);
            }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(List<ICLPPageObject>), () => new List<ICLPPageObject>());

        /// <summary>
        /// The old dimensions of all the arrays on the page indexed by ID number.
        /// </summary>
        public Dictionary<string, Point> OldDimensions
        {
            get
            {
                return GetValue<Dictionary<string, Point>>(OldDimensionsProperty);
            }
            set
            {
                SetValue(OldDimensionsProperty, value);
            }
        }

        /// <summary>
        /// Register the OldDimensions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OldDimensionsProperty = RegisterProperty("OldDimensions", typeof(Dictionary<string, Point>), null);

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            foreach(var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByUniqueID(pageObjectID)))
            {
                try
                {
                    ParentPage.PageObjects.Remove(pageObject);
                    PageObjects.Add(pageObject);
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteErrorToLog("Undo PageObjectsMassAdd Error.", ex);
                }
            }

            Dictionary<string, Point> newDimensions = new Dictionary<string, Point>();
            foreach(KeyValuePair<string, Point> entry in OldDimensions)
            {
                var array = ParentPage.GetPageObjectByUniqueID(entry.Key);
                Point dimensions = entry.Value;
                if(array is CLPArray)
                {
                    newDimensions.Add(entry.Key, new Point(array.Width, array.Height));

                    array.Width = dimensions.X;
                    array.Height = dimensions.Y;
                    array.OnResized();
                }
            }
            OldDimensions = newDimensions;

            //If FFC with remainder on page, update
            foreach(var pageObject in ParentPage.PageObjects)
            {
                if(pageObject is CLPFuzzyFactorCard)
                {
                    if((pageObject as CLPFuzzyFactorCard).RemainderRegionUniqueID != null)
                    {
                        CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID((pageObject as CLPFuzzyFactorCard).RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                        remainderRegion.UpdateTiles();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(!PageObjects.Any())
            {
                Logger.Instance.WriteToLog("PageObjectsMassAdd Redo Failure: No objects to add.");
                return;
            }

            foreach(var pageObject in PageObjects)
            {
                pageObject.ParentPage = ParentPage;
                ParentPage.PageObjects.Add(pageObject);
            }

            PageObjects.Clear(); //no sense storing the actual pageObjects for serialization if it's on the page.

            Dictionary<string, Point> newDimensions = new Dictionary<string, Point>();
            foreach(KeyValuePair<string, Point> entry in OldDimensions)
            {
                var array = ParentPage.GetPageObjectByUniqueID(entry.Key);
                Point dimensions = entry.Value;
                if(array is CLPArray)
                {
                    newDimensions.Add(entry.Key, new Point(array.Width, array.Height));

                    array.Width = dimensions.X;
                    array.Height = dimensions.Y;
                    array.OnResized();
                }
            }
            OldDimensions = newDimensions;

            //If FFC with remainder on page, update
            foreach(var pageObject in ParentPage.PageObjects)
            {
                if(pageObject is CLPFuzzyFactorCard)
                {
                    if((pageObject as CLPFuzzyFactorCard).RemainderRegionUniqueID != null)
                    {
                        CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID((pageObject as CLPFuzzyFactorCard).RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                        remainderRegion.UpdateTiles();
                        break;
                    }
                }
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryArrayMassAddMassResize;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.PageObjects.Clear();
            foreach(var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByUniqueID(pageObjectID)))
            {
                clonedHistoryItem.PageObjects.Add(pageObject);
            }

            Dictionary<string, Point> newDimensions = new Dictionary<string, Point>();
            foreach(KeyValuePair<string, Point> entry in OldDimensions)
            {
                var array = ParentPage.GetPageObjectByUniqueID(entry.Key);
                if(array is CLPArray)
                {
                    newDimensions.Add(entry.Key, new Point(array.Width, array.Height));
                }
            }
            clonedHistoryItem.OldDimensions = newDimensions;

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
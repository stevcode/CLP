using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryCutBatch : ACLPHistoryItemBase, IHistoryBatch
    {
        #region Constructors

        public CLPHistoryCutBatch(ICLPPage parentPage, Stroke cuttingStroke, List<ICLPPageObject> cutPageObjects)
            : base(parentPage)
        {
            CutPageObjects = cutPageObjects;
            foreach(var cutPageObject in cutPageObjects)
            {
                CutPageObjectIDs.Add(cutPageObject.UniqueID);
            }
            SerializedCuttingStroke = new StrokeDTO(cuttingStroke);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryCutBatch(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The Stroke used to cut the objects, serialized for saving.
        /// </summary>
        public StrokeDTO SerializedCuttingStroke
        {
            get { return GetValue<StrokeDTO>(SerializedCuttingStrokeProperty); }
            set { SetValue(SerializedCuttingStrokeProperty, value); }
        }

        public static readonly PropertyData SerializedCuttingStrokeProperty = RegisterProperty("SerializedCuttingStroke", typeof(StrokeDTO));

        /// <summary>
        /// The IDs of all pageObjects cut. Used to locate pageObjects after an undo.
        /// </summary>
        public List<string> CutPageObjectIDs
        {
            get { return GetValue<List<string>>(CutPageObjectIDsProperty); }
            set { SetValue(CutPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData CutPageObjectIDsProperty = RegisterProperty("CutPageObjectIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// A list of all pageObjects removed from the page during a cut. To be restored on Undo. Null after Undo.
        /// </summary>
        public List<ICLPPageObject> CutPageObjects
        {
            get { return GetValue<List<ICLPPageObject>>(CutPageObjectsProperty); }
            set { SetValue(CutPageObjectsProperty, value); }
        }

        public static readonly PropertyData CutPageObjectsProperty = RegisterProperty("CutPageObjects", typeof(List<ICLPPageObject>), () => new List<ICLPPageObject>());

        /// <summary>
        /// UniqueIDs of all new pageObjects placed on page after a cut.
        /// </summary>
        public List<string> HalvedPageObjectIDs
        {
            get { return GetValue<List<string>>(HalvedPageObjectIDsProperty); }
            set { SetValue(HalvedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData HalvedPageObjectIDsProperty = RegisterProperty("HalvedPageObjectIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// Populated after a undo so that HalvedPageObjects can be restored on redo. Null after redo.
        /// </summary>
        public List<ICLPPageObject> HalvedPageObjects
        {
            get { return GetValue<List<ICLPPageObject>>(HalvedPageObjectsProperty); }
            set { SetValue(HalvedPageObjectsProperty, value); }
        }

        public static readonly PropertyData HalvedPageObjectsProperty = RegisterProperty("HalvedPageObjects", typeof(List<ICLPPageObject>), () => new List<ICLPPageObject>());

        public int BatchDelay
        {
            get
            {
                return 100;
            }
        }

        public int NumberOfBatchTicks
        {
            get
            {
                //0 = Draw Cutting Stroke
                //1 = Delete CutPageObjects and Add HalvedPageObjects. Does this need to be 2 actions?
                //2 = Remove Cutting Stroke
                return 2;
            }
        }

        /// <summary>
        /// Location within the Batch.
        /// </summary>
        public int CurrentBatchTickIndex
        {
            get { return GetValue<int>(CurrentBatchTickIndexProperty); }
            set { SetValue(CurrentBatchTickIndexProperty, value); }
        }

        public static readonly PropertyData CurrentBatchTickIndexProperty = RegisterProperty("CurrentBatchTickIndex", typeof(int), 0);

        #endregion //Properties

        #region Methods

        public void AddHalvedPageObjectIDsToBatch(List<string> halvedPageObjectIDs)
        {
            HalvedPageObjectIDs = halvedPageObjectIDs;
        }

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if(CurrentBatchTickIndex <= 0)
            {
                CurrentBatchTickIndex = 0;
                return;
            }

            if(isAnimationUndo)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex - 1];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex--;
            }
            else if(HalvedPageObjectIDs.Any())
            {
                var halvedPageObjects = new List<ICLPPageObject>();
                foreach(var pageObject in HalvedPageObjectIDs.Select(halvedPageObjectID => ParentPage.GetPageObjectByUniqueID(halvedPageObjectID))) 
                {
                    halvedPageObjects.Add(pageObject);
                    ParentPage.PageObjects.Remove(pageObject);
                }
                HalvedPageObjects = halvedPageObjects;

                foreach(var cutPageObject in CutPageObjects)
                {
                    ParentPage.PageObjects.Add(cutPageObject);
                    cutPageObject.RefreshStrokeParentIDs();
                }
                CutPageObjects = null;

                CurrentBatchTickIndex = 0;
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(CurrentBatchTickIndex > NumberOfBatchTicks)
            {
                return;
            }

            if(isAnimationRedo)
            {
                var stretchedDimension = StretchedDimensions[CurrentBatchTickIndex];
                pageObject.Width = stretchedDimension.X;
                pageObject.Height = stretchedDimension.Y;
                CurrentBatchTickIndex++;
            }
            else if(CutPageObjectIDs.Any())
            {
                var cutPageObjects = new List<ICLPPageObject>();
                foreach(var pageObject in CutPageObjectIDs.Select(cutPageObjectID => ParentPage.GetPageObjectByUniqueID(cutPageObjectID)))
                {
                    cutPageObjects.Add(pageObject);
                    ParentPage.PageObjects.Remove(pageObject);
                }
                CutPageObjects = cutPageObjects;

                foreach(var halvedPageObject in HalvedPageObjects)
                {
                    ParentPage.PageObjects.Add(halvedPageObject);
                    halvedPageObject.RefreshStrokeParentIDs();
                }
                HalvedPageObjects = null;

                CurrentBatchTickIndex = NumberOfBatchTicks;
            }
        }

        public void ClearBatchAfterCurrentIndex()
        {
            CurrentBatchTickIndex = 0;
            //clear both lists
        }

        #endregion //Methods
    }
}

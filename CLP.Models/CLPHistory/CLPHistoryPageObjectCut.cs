using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Threading;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectCut : ACLPHistoryItemBase
    {
        private const int STROKE_CUT_DELAY = 375;

        #region Constructors

        public CLPHistoryPageObjectCut(ICLPPage parentPage, Stroke cuttingStroke, List<ICLPPageObject> cutPageObjects, List<string> halvedPageObjectIDs)
            : base(parentPage)
        {
            SerializedCuttingStroke = new StrokeDTO(cuttingStroke);
            CutPageObjects = cutPageObjects;
            foreach(var cutPageObject in cutPageObjects)
            {
                CutPageObjectIDs.Add(cutPageObject.UniqueID);
            }
            HalvedPageObjectIDs = halvedPageObjectIDs;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectCut(SerializationInfo info, StreamingContext context)
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
            if(!HalvedPageObjectIDs.Any())
            {
                return;
            }

            var cuttingStroke = SerializedCuttingStroke.ToStroke();
            if(isAnimationUndo)
            {
                ParentPage.InkStrokes.Add(cuttingStroke);
                Wait(STROKE_CUT_DELAY);
            }
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
                //cutPageObject.RefreshStrokeParentIDs(); //TODO: find way to do this after CutStroke removal if is an animation.
            }
            CutPageObjects = null;
            if(isAnimationUndo)
            {
                Wait(STROKE_CUT_DELAY);
                ParentPage.InkStrokes.Remove(cuttingStroke);
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(!CutPageObjectIDs.Any())
            {
                return;
            }

            var cuttingStroke = SerializedCuttingStroke.ToStroke();
            if(isAnimationRedo)
            {
                ParentPage.InkStrokes.Add(cuttingStroke);
                Wait(STROKE_CUT_DELAY);
            }
            var cutPageObjects = new List<ICLPPageObject>();
            foreach(var pageObject in CutPageObjectIDs.Select(cutPageObjectID => ParentPage.GetPageObjectByUniqueID(cutPageObjectID)))
            {
                cutPageObjects.Add(pageObject);
                ParentPage.PageObjects.Remove(pageObject);
            }
            CutPageObjects = cutPageObjects;

            foreach(var halvedPageObject in HalvedPageObjects)
            {
                halvedPageObject.ParentPage = ParentPage;
                ParentPage.PageObjects.Add(halvedPageObject);
                //halvedPageObject.RefreshStrokeParentIDs(); //TODO: find way to do this after CutStroke removal if is an animation.
            }
            HalvedPageObjects = null;
            if(isAnimationRedo)
            {
                Wait(STROKE_CUT_DELAY);
                ParentPage.InkStrokes.Remove(cuttingStroke);
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPageObjectCut;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            if(!HalvedPageObjectIDs.Any())
            {
                Logger.Instance.WriteToLog("Empty HalvedPageObjectIDs in CLPHistoryPageObjectCut during UndoRedoCompleteClone.");
                return null;
            }

            var halvedPageObjects = HalvedPageObjectIDs.Select(halvedPageObjectID => ParentPage.GetPageObjectByUniqueID(halvedPageObjectID)).ToList();
            clonedHistoryItem.HalvedPageObjects = halvedPageObjects;

            return clonedHistoryItem;
        }

        private void Wait(int timeToWait)
        {
            var frame = new DispatcherFrame();
            new Thread(() =>
            {
                Thread.Sleep(timeToWait);
                frame.Continue = false;
            }).Start();
            Dispatcher.PushFrame(frame);
        }

        #endregion //Methods
    }
}

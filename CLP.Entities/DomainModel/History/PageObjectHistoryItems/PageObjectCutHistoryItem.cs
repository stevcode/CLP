using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PageObjectCutHistoryItem : AHistoryItemBase
    {
        private const int STROKE_CUT_DELAY = 375;

        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageObjectCutHistoryItem" /> from scratch.
        /// </summary>
        public PageObjectCutHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="PageObjectCutHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public PageObjectCutHistoryItem(CLPPage parentPage, Person owner, Stroke cuttingStroke, IEnumerable<ICuttable> cutPageObjects, List<string> halvedPageObjectIDs)
            : base(parentPage, owner)
        {
            CuttingStrokeID = cuttingStroke.GetStrokeID();
            if(!ParentPage.History.TrashedInkStrokes.Contains(cuttingStroke))
            {
                ParentPage.History.TrashedInkStrokes.Add(cuttingStroke);
            }
            HalvedPageObjectIDs = halvedPageObjectIDs;
            foreach(var cutPageObject in cutPageObjects)
            {
                CutPageObjectIDs.Add(cutPageObject.ID);
                ParentPage.History.TrashedPageObjects.Add(cutPageObject);
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PageObjectCutHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The ID of the <see cref="Stroke" /> used to cut the objects.
        /// </summary>
        public string CuttingStrokeID
        {
            get { return GetValue<string>(CuttingStrokeIDProperty); }
            set { SetValue(CuttingStrokeIDProperty, value); }
        }

        public static readonly PropertyData CuttingStrokeIDProperty = RegisterProperty("CuttingStrokeID", typeof(string));

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
        /// UniqueIDs of all new pageObjects placed on page after a cut.
        /// </summary>
        public List<string> HalvedPageObjectIDs
        {
            get { return GetValue<List<string>>(HalvedPageObjectIDsProperty); }
            set { SetValue(HalvedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData HalvedPageObjectIDsProperty = RegisterProperty("HalvedPageObjectIDs", typeof(List<string>));

        /// <summary>
        /// List of the Halved <see cref="IPageObject" />s to be used on another machine when <see cref="PageObjectCutHistoryItem" /> is unpacked.
        /// </summary>
        [XmlIgnore]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        /// <summary>
        /// Serialized <see cref="Stroke" /> used to cut the <see cref="ICuttable" />.
        /// </summary>
        [XmlIgnore]
        public StrokeDTO PackagedCuttingStroke
        {
            get { return GetValue<StrokeDTO>(PackagedCuttingStrokeProperty); }
            set { SetValue(PackagedCuttingStrokeProperty, value); }
        }

        public static readonly PropertyData PackagedCuttingStrokeProperty = RegisterProperty("PackagedCuttingStroke", typeof(StrokeDTO));

        public override string FormattedValue
        {
            get
            {
                List<string> PageObjectTypes = new List<string>();
                try
                {
                    foreach(var pageObject in CutPageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByID(pageObjectID)))
                    {
                        PageObjectTypes.Add(pageObject.GetType().ToString());
                    }
                }
                catch(Exception e)
                {
                }
                
                string formattedValue = string.Format("Index # {0}, Cut {1} on page.", HistoryIndex, string.Join(", ", PageObjectTypes));
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
            if(!HalvedPageObjectIDs.Any())
            {
                return;
            }

            try
            {
                var cuttingStroke = ParentPage.History.GetStrokeByID(CuttingStrokeID);
                if(isAnimationUndo)
                {
                    ParentPage.InkStrokes.Add(cuttingStroke);
                    PageHistory.UISleep(STROKE_CUT_DELAY);
                }
                foreach(var halvedPageObject in HalvedPageObjectIDs.Select(halvedPageObjectID => ParentPage.GetPageObjectByID(halvedPageObjectID)))
                {
                    ParentPage.PageObjects.Remove(halvedPageObject);
                    ParentPage.History.TrashedPageObjects.Add(halvedPageObject);
                }

                foreach(var cutPageObject in CutPageObjectIDs.Select(cutPageObjectID => ParentPage.History.GetPageObjectByID(cutPageObjectID)))
                {
                    ParentPage.History.TrashedPageObjects.Remove(cutPageObject);
                    ParentPage.PageObjects.Add(cutPageObject);
                    //cutPageObject.RefreshStrokeParentIDs(); //TODO: find way to do this after CutStroke removal if is an animation.
                }
                if(isAnimationUndo)
                {
                    PageHistory.UISleep(STROKE_CUT_DELAY);
                    ParentPage.InkStrokes.Remove(cuttingStroke);
                }
            }
            catch(Exception e)
            {
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

            try
            {
                var cuttingStroke = ParentPage.History.GetStrokeByID(CuttingStrokeID);
                if(isAnimationRedo)
                {
                    ParentPage.InkStrokes.Add(cuttingStroke);
                    PageHistory.UISleep(STROKE_CUT_DELAY);
                }
                foreach(var cutPageObject in CutPageObjectIDs.Select(cutPageObjectID => ParentPage.GetPageObjectByID(cutPageObjectID)))
                {
                    ParentPage.PageObjects.Remove(cutPageObject);
                    ParentPage.History.TrashedPageObjects.Add(cutPageObject);
                }

                foreach(var halvedPageObject in HalvedPageObjectIDs.Select(halvedPageObjectID => ParentPage.History.GetPageObjectByID(halvedPageObjectID)))
                {
                    ParentPage.History.TrashedPageObjects.Remove(halvedPageObject);
                    ParentPage.PageObjects.Add(halvedPageObject);
                    //halvedPageObject.RefreshStrokeParentIDs(); //TODO: find way to do this after CutStroke removal if is an animation.
                }
                if(isAnimationRedo)
                {
                    PageHistory.UISleep(STROKE_CUT_DELAY);
                    ParentPage.InkStrokes.Remove(cuttingStroke);
                }
            }
            catch(Exception e)
            {
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as PageObjectCutHistoryItem;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.PackagedCuttingStroke = ParentPage.History.GetStrokeByID(CuttingStrokeID).ToStrokeDTO();

            clonedHistoryItem.PackagedPageObjects.Clear();
            foreach(var pageObject in HalvedPageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByID(pageObjectID)))
            {
                try
                {
                    clonedHistoryItem.PackagedPageObjects.Add(pageObject);
                }
                catch(Exception ex) { }
            }

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem()
        {
            ParentPage.History.TrashedInkStrokes.Add(PackagedCuttingStroke.ToStroke());
            foreach(var packagedPageObject in PackagedPageObjects)
            {
                ParentPage.History.TrashedPageObjects.Add(packagedPageObject);
            }
        }

        public override bool IsUsingTrashedPageObject(string id, bool isUndoItem)
        {
            return isUndoItem ? CutPageObjectIDs.Contains(id) : HalvedPageObjectIDs.Contains(id);
        }

        public override bool IsUsingTrashedInkStroke(string id, bool isUndoItem)
        {
            return CuttingStrokeID == id;
        }

        #endregion //Methods
    }
}
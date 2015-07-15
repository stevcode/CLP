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

        /// <summary>Initializes <see cref="PageObjectCutHistoryItem" /> from scratch.</summary>
        public PageObjectCutHistoryItem() { }

        /// <summary>Initializes <see cref="PageObjectCutHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public PageObjectCutHistoryItem(CLPPage parentPage, Person owner, Stroke cuttingStroke, ICuttable cutPageObject, List<string> halvedPageObjectIDs)
            : base(parentPage, owner)
        {
            CuttingStrokeID = cuttingStroke.GetStrokeID();
            if (!parentPage.History.TrashedInkStrokes.Contains(cuttingStroke))
            {
                parentPage.History.TrashedInkStrokes.Add(cuttingStroke);
            }

            if (cutPageObject != null)
            {
                CutPageObjectID = cutPageObject.ID;
                if (!parentPage.History.TrashedPageObjects.Contains(cutPageObject))
                {
                    parentPage.History.TrashedPageObjects.Add(cutPageObject);
                }
            }

            HalvedPageObjectIDs = halvedPageObjectIDs;

            CachedFormattedValue = FormattedValue;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PageObjectCutHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>The ID of the <see cref="Stroke" /> used to cut the objects.</summary>
        public string CuttingStrokeID
        {
            get { return GetValue<string>(CuttingStrokeIDProperty); }
            set { SetValue(CuttingStrokeIDProperty, value); }
        }

        public static readonly PropertyData CuttingStrokeIDProperty = RegisterProperty("CuttingStrokeID", typeof (string));

        /// <summary>The IDs of all pageObjects cut. Used to locate pageObjects after an undo.</summary>
        [Obsolete("Only allow cutting stroke to cut one pageObject at a time, use CutPageObjectID now.")]
        public List<string> CutPageObjectIDs
        {
            get { return GetValue<List<string>>(CutPageObjectIDsProperty); }
            set { SetValue(CutPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData CutPageObjectIDsProperty = RegisterProperty("CutPageObjectIDs", typeof (List<string>), () => new List<string>());

        /// <summary>ID of the pageObject that was cut.</summary>
        public string CutPageObjectID
        {
            get { return GetValue<string>(CutPageObjectIDProperty); }
            set { SetValue(CutPageObjectIDProperty, value); }
        }

        public static readonly PropertyData CutPageObjectIDProperty = RegisterProperty("CutPageObjectID", typeof (string), string.Empty);

        /// <summary>UniqueIDs of all new pageObjects placed on page after a cut.</summary>
        public List<string> HalvedPageObjectIDs
        {
            get { return GetValue<List<string>>(HalvedPageObjectIDsProperty); }
            set { SetValue(HalvedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData HalvedPageObjectIDsProperty = RegisterProperty("HalvedPageObjectIDs", typeof (List<string>));

        /// <summary>List of the Halved <see cref="IPageObject" />s to be used on another machine when <see cref="PageObjectCutHistoryItem" /> is unpacked.</summary>
        [XmlIgnore]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        /// <summary>Serialized <see cref="Stroke" /> used to cut the <see cref="ICuttable" />.</summary>
        [XmlIgnore]
        public StrokeDTO PackagedCuttingStroke
        {
            get { return GetValue<StrokeDTO>(PackagedCuttingStrokeProperty); }
            set { SetValue(PackagedCuttingStrokeProperty, value); }
        }

        public static readonly PropertyData PackagedCuttingStrokeProperty = RegisterProperty("PackagedCuttingStroke", typeof (StrokeDTO));

        public override string FormattedValue
        {
            get
            {
                var cutPageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(CutPageObjectID);
                return cutPageObject == null
                           ? string.Format("Index #{0}, Nothing Cut.", HistoryIndex)
                           : string.Format("Index #{0}, Cut {1}.", HistoryIndex, cutPageObject.FormattedName);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            if (!HalvedPageObjectIDs.Any())
            {
                CutPageObjectID = string.Empty;
            }

            UndoAction(false);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if (!HalvedPageObjectIDs.Any())
            {
                CutPageObjectID = string.Empty;
            }

            var cutPageObject = ParentPage.GetVerifiedPageObjectInTrashByID(CutPageObjectID);
            var halvedPageObjects = HalvedPageObjectIDs.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
            halvedPageObjects = halvedPageObjects.Where(p => p != null).ToList();

            var cuttingStroke = ParentPage.GetVerifiedStrokeInHistoryByID(CuttingStrokeID);
            if (cuttingStroke == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Cutting Stroke not found on page or in history.", HistoryIndex);
                return;
            }

            if (isAnimationUndo)
            {
                ParentPage.InkStrokes.Add(cuttingStroke);
                PageHistory.UISleep(STROKE_CUT_DELAY);
            }

            foreach (var halvedPageObject in halvedPageObjects)
            {
                ParentPage.PageObjects.Remove(halvedPageObject);
                ParentPage.History.TrashedPageObjects.Add(halvedPageObject);
            }
            if (cutPageObject != null)
            {
                ParentPage.History.TrashedPageObjects.Remove(cutPageObject);
                ParentPage.PageObjects.Add(cutPageObject);
            }

            if (isAnimationUndo)
            {
                //PageHistory.UISleep(STROKE_CUT_DELAY);
                ParentPage.InkStrokes.Remove(cuttingStroke);
            }

            if (!halvedPageObjects.Any() ||
                cutPageObject == null)
            {
                return;
            }

            AStrokeAccepter.SplitAcceptedStrokes(halvedPageObjects,
                                                 new List<IPageObject>
                                                 {
                                                     cutPageObject
                                                 });
            APageObjectAccepter.SplitAcceptedPageObjects(halvedPageObjects,
                                                         new List<IPageObject>
                                                         {
                                                             cutPageObject
                                                         });
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if (!HalvedPageObjectIDs.Any())
            {
                CutPageObjectID = string.Empty;
            }

            var cutPageObject = ParentPage.GetVerifiedPageObjectOnPageByID(CutPageObjectID);
            var halvedPageObjects = HalvedPageObjectIDs.Select(id => ParentPage.GetVerifiedPageObjectInTrashByID(id)).ToList();
            halvedPageObjects = halvedPageObjects.Where(p => p != null).ToList();

            var cuttingStroke = ParentPage.GetVerifiedStrokeInHistoryByID(CuttingStrokeID);
            if (cuttingStroke == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Cutting Stroke not found on page or in history.", HistoryIndex);
                return;
            }

            if (isAnimationRedo)
            {
                ParentPage.InkStrokes.Add(cuttingStroke);
                PageHistory.UISleep(STROKE_CUT_DELAY);
            }

            if (cutPageObject != null)
            {
                ParentPage.PageObjects.Remove(cutPageObject);
                ParentPage.History.TrashedPageObjects.Add(cutPageObject);
            }
            foreach (var halvedPageObject in halvedPageObjects)
            {
                ParentPage.History.TrashedPageObjects.Remove(halvedPageObject);
                ParentPage.PageObjects.Add(halvedPageObject);
            }

            if (isAnimationRedo)
            {
                //PageHistory.UISleep(STROKE_CUT_DELAY);
                ParentPage.InkStrokes.Remove(cuttingStroke);
            }

            if (!halvedPageObjects.Any() ||
                cutPageObject == null)
            {
                return;
            }

            AStrokeAccepter.SplitAcceptedStrokes(new List<IPageObject>
                                                 {
                                                     cutPageObject
                                                 },
                                                 halvedPageObjects);
            APageObjectAccepter.SplitAcceptedPageObjects(new List<IPageObject>
                                                         {
                                                             cutPageObject
                                                         },
                                                         halvedPageObjects);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as PageObjectCutHistoryItem;
            if (clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.PackagedCuttingStroke = ParentPage.History.GetStrokeByID(CuttingStrokeID).ToStrokeDTO();

            clonedHistoryItem.PackagedPageObjects.Clear();
            foreach (var pageObject in HalvedPageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByID(pageObjectID)))
            {
                try
                {
                    clonedHistoryItem.PackagedPageObjects.Add(pageObject);
                }
                catch (Exception) { }
            }

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem()
        {
            ParentPage.History.TrashedInkStrokes.Add(PackagedCuttingStroke.ToStroke());
            foreach (var packagedPageObject in PackagedPageObjects)
            {
                ParentPage.History.TrashedPageObjects.Add(packagedPageObject);
            }
        }

        public override bool IsUsingTrashedPageObject(string id) { return CutPageObjectID == id || HalvedPageObjectIDs.Contains(id); }

        public override bool IsUsingTrashedInkStroke(string id) { return CuttingStrokeID == id; }

        #endregion //Methods
    }
}
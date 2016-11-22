using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public class PageObjectCutHistoryAction : AHistoryActionBase
    {
        #region Constants

        private const int STROKE_CUT_DELAY = 375;

        #endregion // Constants

        #region Constructors

        /// <summary>Initializes <see cref="PageObjectCutHistoryAction" /> from scratch.</summary>
        public PageObjectCutHistoryAction() { }

        /// <summary>Initializes <see cref="PageObjectCutHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public PageObjectCutHistoryAction(CLPPage parentPage, Person owner, Stroke cuttingStroke, ICuttable cutPageObject, List<string> halvedPageObjectIDs)
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
        }

        #endregion // Constructors

        #region Properties

        /// <summary>The ID of the <see cref="Stroke" /> used to cut the objects.</summary>
        public string CuttingStrokeID
        {
            get { return GetValue<string>(CuttingStrokeIDProperty); }
            set { SetValue(CuttingStrokeIDProperty, value); }
        }

        public static readonly PropertyData CuttingStrokeIDProperty = RegisterProperty("CuttingStrokeID", typeof(string));

        /// <summary>ID of the pageObject that was cut.</summary>
        public string CutPageObjectID
        {
            get { return GetValue<string>(CutPageObjectIDProperty); }
            set { SetValue(CutPageObjectIDProperty, value); }
        }

        public static readonly PropertyData CutPageObjectIDProperty = RegisterProperty("CutPageObjectID", typeof(string), string.Empty);

        /// <summary>UniqueIDs of all new pageObjects placed on page after a cut.</summary>
        public List<string> HalvedPageObjectIDs
        {
            get { return GetValue<List<string>>(HalvedPageObjectIDsProperty); }
            set { SetValue(HalvedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData HalvedPageObjectIDsProperty = RegisterProperty("HalvedPageObjectIDs", typeof(List<string>));

        /// <summary>List of the Halved <see cref="IPageObject" />s to be used on another machine when <see cref="PageObjectCutHistoryAction" /> is unpacked.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        /// <summary>Serialized <see cref="Stroke" /> used to cut the <see cref="ICuttable" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public StrokeDTO PackagedCuttingStroke
        {
            get { return GetValue<StrokeDTO>(PackagedCuttingStrokeProperty); }
            set { SetValue(PackagedCuttingStrokeProperty, value); }
        }

        public static readonly PropertyData PackagedCuttingStrokeProperty = RegisterProperty("PackagedCuttingStroke", typeof(StrokeDTO));

        #endregion // Properties

        #region AHistoryActionBase Overrides

        protected override string FormattedReport
        {
            get
            {
                var cutPageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(CutPageObjectID);
                return cutPageObject == null ? "[ERROR] Cut PageObject not found on page or in history." : $"Cut {cutPageObject.FormattedName}.";
            }
        }

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
                Console.WriteLine("[ERROR] on Index #{0}, Cutting Stroke not found on page or in history.", HistoryActionIndex);
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
                Console.WriteLine("[ERROR] on Index #{0}, Cutting Stroke not found on page or in history.", HistoryActionIndex);
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

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }

            clonedHistoryAction.PackagedCuttingStroke = ParentPage.History.GetStrokeByID(CuttingStrokeID).ToStrokeDTO();

            clonedHistoryAction.PackagedPageObjects.Clear();
            foreach (var pageObject in HalvedPageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByID(pageObjectID)))
            {
                try
                {
                    clonedHistoryAction.PackagedPageObjects.Add(pageObject);
                }
                catch (Exception) { }
            }

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction()
        {
            ParentPage.History.TrashedInkStrokes.Add(PackagedCuttingStroke.ToStroke());
            foreach (var packagedPageObject in PackagedPageObjects)
            {
                ParentPage.History.TrashedPageObjects.Add(packagedPageObject);
            }
        }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return CutPageObjectID == id || HalvedPageObjectIDs.Contains(id);
        }

        public override bool IsUsingTrashedInkStroke(string id)
        {
            return CuttingStrokeID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}
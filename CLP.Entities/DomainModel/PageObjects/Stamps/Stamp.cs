using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum StampTypes
    {
        GeneralStamp,
        ObservingStamp,
        GroupStamp,
        EmptyGroupStamp
    }

    [Serializable]
    public class Stamp : APageObjectBase, ICountable, IStrokeAccepter, IPageObjectAccepter, IReporter
    {
        #region Constructors

        /// <summary>Initializes <see cref="Stamp" /> from scratch.</summary>
        public Stamp() { }

        /// <summary>Initializes <see cref="Stamp" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Stamp" /> belongs to.</param>
        public Stamp(CLPPage parentPage, string imageHashID, StampTypes stampType)
            : base(parentPage)
        {
            CanAcceptPageObjects = stampType == StampTypes.GroupStamp;
            StampType = stampType;

            switch (stampType)
            {
                case StampTypes.GeneralStamp:
                    XPosition = 800;
                    YPosition = 90;
                    Width = 75;
                    Height = 180;
                    break;
                case StampTypes.ObservingStamp:
                    Width = 90;
                    Height = 140;
                    break;
                case StampTypes.GroupStamp:
                    XPosition = 800;
                    YPosition = 90;
                    Width = 125;
                    Height = 230;
                    break;
                case StampTypes.EmptyGroupStamp:
                    Width = 200;
                    Height = 230;
                    break;
                default:
                    Width = 75;
                    Height = 180;
                    break;
            }

            ImageHashID = imageHashID;
            if (stampType == StampTypes.ObservingStamp)
            {
                Parts = 1;
            }
        }

        /// <summary>Initializes <see cref="Stamp" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Stamp" /> belongs to.</param>
        public Stamp(CLPPage parentPage, StampTypes stampType)
            : this(parentPage, string.Empty, stampType) { }

        /// <summary>Initializes <see cref="Stamp" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Stamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public virtual double HandleHeight
        {
            get { return StampType == StampTypes.GeneralStamp || StampType == StampTypes.GroupStamp ? 35 : 0; }
        }

        public virtual double PartsHeight
        {
            get
            {
                switch (StampType)
                {
                    case StampTypes.GeneralStamp:
                        return 70;
                    case StampTypes.ObservingStamp:
                        return 65;
                    case StampTypes.GroupStamp:
                        return 70;
                    case StampTypes.EmptyGroupStamp:
                        return 30;
                    default:
                        return 70;
                }
            }
        }

        /// <summary>The unique Hash of the image this <see cref="Stamp" /> contains.</summary>
        public string ImageHashID
        {
            get { return GetValue<string>(ImageHashIDProperty); }
            set { SetValue(ImageHashIDProperty, value); }
        }

        public static readonly PropertyData ImageHashIDProperty = RegisterProperty("ImageHashID", typeof (string), string.Empty);

        /// <summary>The type of <see cref="Stamp" />.</summary>
        public StampTypes StampType
        {
            get { return GetValue<StampTypes>(StampTypeProperty); }
            set { SetValue(StampTypeProperty, value); }
        }

        public static readonly PropertyData StampTypeProperty = RegisterProperty("StampType", typeof (StampTypes), StampTypes.GeneralStamp);

        #region ICountable Members

        /// <summary>Number of parts the <see cref="Stamp" /> represents.</summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int), 0);

        /// <summary>Is an <see cref="ICountable" /> that doesn't accept inner parts.</summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof (bool), false);

        /// <summary>Parts is Auto-Generated and non-modifiable (except under special circumstances).</summary>
        public bool IsPartsAutoGenerated
        {
            get { return GetValue<bool>(IsPartsAutoGeneratedProperty); }
            set { SetValue(IsPartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData IsPartsAutoGeneratedProperty = RegisterProperty("IsPartsAutoGenerated", typeof (bool), false);

        #endregion

        #region IStrokeAccepter Members

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof (bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes",
                                                                                       typeof (List<Stroke>),
                                                                                       () => new List<Stroke>());

        /// <summary>The IDs of the <see cref="Stroke" />s that have been accepted.</summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs",
                                                                                               typeof (List<string>),
                                                                                               () => new List<string>());

        #endregion //IStrokeAccepter Members

        #region IPageObjectAccepter Members

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="IPageObject" />s.</summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof (bool), false);

        /// <summary>The currently accepted <see cref="IPageObject" />s.</summary>
        [XmlIgnore]
        public List<IPageObject> AcceptedPageObjects
        {
            get { return GetValue<List<IPageObject>>(AcceptedPageObjectsProperty); }
            set { SetValue(AcceptedPageObjectsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectsProperty = RegisterProperty("AcceptedPageObjects",
                                                                                           typeof (List<IPageObject>),
                                                                                           () => new List<IPageObject>());

        /// <summary>The IDs of the <see cref="IPageObject" />s that have been accepted.</summary>
        public List<string> AcceptedPageObjectIDs
        {
            get { return GetValue<List<string>>(AcceptedPageObjectIDsProperty); }
            set { SetValue(AcceptedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectIDsProperty = RegisterProperty("AcceptedPageObjectIDs",
                                                                                             typeof (List<string>),
                                                                                             () => new List<string>());

        #endregion //IPageObjectAccepter Members

        #region IReporter Members

        private int NumberInGroups
        {
            get
            {
                if (ParentPage == null)
                {
                    return 0;
                }

                var childStampedObjects = ParentPage.PageObjects.OfType<StampedObject>().Where(x => x.ParentStampID == ID).ToList();
                var groupStampedObjects =
                    ParentPage.PageObjects.OfType<StampedObject>()
                              .Where(
                                     x =>
                                     (x.StampedObjectType == StampedObjectTypes.GroupStampedObject ||
                                      x.StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject) && x.Parts > 0)
                              .ToList();

                var groupedStampedObjects =
                    childStampedObjects.Where(c => groupStampedObjects.Count(x => x.AcceptedPageObjectIDs.Contains(c.ID)) > 0).ToList();

                return groupedStampedObjects.Count;
            }
        }

        private int NumberNotInGroups
        {
            get
            {
                if (ParentPage == null)
                {
                    return 0;
                }

                var childStampedObjects = ParentPage.PageObjects.OfType<StampedObject>().Where(x => x.ParentStampID == ID).ToList();
                var groupStampedObjects =
                    ParentPage.PageObjects.OfType<StampedObject>()
                              .Where(
                                     x =>
                                     (x.StampedObjectType == StampedObjectTypes.GroupStampedObject ||
                                      x.StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject) && x.Parts > 0)
                              .ToList();

                var groupedStampedObjects =
                    childStampedObjects.Where(c => groupStampedObjects.Count(x => x.AcceptedPageObjectIDs.Contains(c.ID)) > 0).ToList();

                return childStampedObjects.Count - groupedStampedObjects.Count;
            }
        }

        /// <summary>Formatted value of the <see cref="IReporter" />.</summary>
        public string FormattedReport
        {
            get
            {
                return StampType != StampTypes.ObservingStamp
                           ? string.Empty
                           : string.Format("{0} in groups\n" + "{1} not in groups", NumberInGroups, NumberNotInGroups);
            }
        }

        #endregion //IReporter Members

        #endregion //Properties

        #region Methods

        public override void OnRestoredFromHistory()
        {
            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            //BUG: This is going to cause issues if you use Undo or Redo while not in Select Mode
            ParentPage.InkStrokes.Add(new StrokeCollection(AcceptedStrokes));
            ParentPage.History.TrashedInkStrokes.Remove(new StrokeCollection(AcceptedStrokes));
        }

        public override void OnDeleted()
        {
            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            ParentPage.InkStrokes.Remove(new StrokeCollection(AcceptedStrokes));
            ParentPage.History.TrashedInkStrokes.Add(new StrokeCollection(AcceptedStrokes));
        }

        public override void OnResizing(double oldWidth, double oldHeight) { }

        public override void OnResized(double oldWidth, double oldHeight) { OnResizing(oldWidth, oldHeight); }

        public override void OnMoving(double oldX, double oldY)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if (CanAcceptStrokes)
            {
                foreach (var stroke in AcceptedStrokes)
                {
                    var transform = new Matrix();
                    transform.Translate(deltaX, deltaY);
                    stroke.Transform(transform, true);
                }
            }

            if (CanAcceptPageObjects)
            {
                foreach (var pageObject in AcceptedPageObjects)
                {
                    pageObject.XPosition += deltaX;
                    pageObject.YPosition += deltaY;
                }
            }
        }

        public override IPageObject Duplicate()
        {
            var newStamp = Clone() as Stamp;
            if (newStamp == null)
            {
                return null;
            }
            newStamp.CreationDate = DateTime.Now;
            newStamp.ID = Guid.NewGuid().ToCompactID();
            newStamp.VersionIndex = 0;
            newStamp.LastVersionIndex = null;
            newStamp.ParentPage = ParentPage;

            return newStamp;
        }

        public override bool PageObjectIsOver(IPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var area = (Height - HandleHeight - PartsHeight) * Width;
            var top = Math.Max(YPosition + HandleHeight, pageObject.YPosition);
            var bottom = Math.Min(YPosition + Height - PartsHeight, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition, pageObject.XPosition);
            var right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && (intersectionArea / areaObject >= .90 || intersectionArea / area >= .90);
        }

        public void RefreshParts()
        {
            Parts = 0;
            foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
            {
                Parts += pageObject.Parts;
            }
        }

        #region IStrokeAccepter Methods

        public void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            foreach (var stroke in removedStrokes.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
            }

            var stampBodyBoundingBox = new Rect(XPosition, YPosition + HandleHeight, Width, Height - HandleHeight - PartsHeight);
            foreach (var stroke in
                addedStrokes.Where(stroke => stroke.HitTest(stampBodyBoundingBox, 50) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }
        }

        public void RefreshAcceptedStrokes()
        {
            AcceptedStrokes.Clear();
            AcceptedStrokeParentIDs.Clear();
            if (!CanAcceptStrokes)
            {
                return;
            }

            var stampBodyBoundingBox = new Rect(XPosition, YPosition + HandleHeight, Width, Height - HandleHeight - PartsHeight);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(stampBodyBoundingBox, 50) //Stroke must be at least 50% contained by Stamp body.
                                    select stroke;

            AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
        }

        #endregion //IStrokeAccepter Methods

        #region IPageObjectAccepter Methods

        public void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in removedPageObjects.Where(pageObject => AcceptedPageObjectIDs.Contains(pageObject.ID)))
            {
                AcceptedPageObjects.Remove(pageObject);
                AcceptedPageObjectIDs.Remove(pageObject.ID);
            }

            foreach (var pageObject in addedPageObjects.OfType<ICountable>())
            {
                if (AcceptedPageObjectIDs.Contains(pageObject.ID) ||
                    pageObject is Stamp ||
                    (pageObject is StampedObject &&
                     ((pageObject as StampedObject).StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject ||
                      (pageObject as StampedObject).StampedObjectType == StampedObjectTypes.GroupStampedObject)))
                {
                    continue;
                }
                AcceptedPageObjects.Add(pageObject);
                AcceptedPageObjectIDs.Add(pageObject.ID);
            }

            RefreshParts();
        }

        public void RefreshAcceptedPageObjects()
        {
            AcceptedPageObjects.Clear();
            AcceptedPageObjectIDs.Clear();
            if (!CanAcceptPageObjects)
            {
                return;
            }

            var pageObjectsOverStamp = from pageObject in ParentPage.PageObjects
                                       where PageObjectIsOver(pageObject, .90) //PageObject must be at least 90% contained by Stamp body.
                                       select pageObject;

            AcceptPageObjects(pageObjectsOverStamp, new List<IPageObject>());
        }

        #endregion //IPageObjectAccepter Methods

        #region IReporter Methods

        public void UpdateReport() { RaisePropertyChanged("FormattedReport"); }

        #endregion //IReporter Methods

        #endregion //Methods
    }
}
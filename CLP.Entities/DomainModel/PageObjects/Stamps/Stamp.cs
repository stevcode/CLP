using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
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
    public class Stamp : AStrokeAndPageObjectAccepter, ICountable, IReporter
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
                    XPosition = 877;
                    YPosition = 25;
                    Width = 95;
                    Height = 190;
                    break;
                case StampTypes.ObservingStamp:
                    XPosition = 730;
                    YPosition = 30;
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
                    XPosition = 850;
                    YPosition = 30;
                    Width = 200;
                    Height = 190;
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

        #endregion //Properties

        #region Methods

        public int GetPartsAtHistoryIndex(int historyIndex)
        {
            var partsHistoryItem =
                ParentPage.History.CompleteOrderedHistoryItems.OfType<PartsValueChangedHistoryItem>().FirstOrDefault(h => h.PageObjectID == ID && h.HistoryIndex >= historyIndex);
            return partsHistoryItem == null ? Parts : partsHistoryItem.PreviousValue;
        }

        #endregion //Methods

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get
            {
                string stampType;
                switch (StampType)
                {
                    case StampTypes.GeneralStamp:
                        stampType = "Stamp";
                        break;
                    case StampTypes.ObservingStamp:
                        stampType = "Object Stamp";
                        break;
                    case StampTypes.GroupStamp:
                        stampType = "Group Stamp";
                        break;
                    case StampTypes.EmptyGroupStamp:
                        stampType = "Empty Group Stamp";
                        break;
                    default:
                        stampType = "Stamp";
                        break;
                }

                return string.Format("{0} with {1} Part(s)", stampType, Parts);
            }
        }

        public override string CodedName
        {
            get { return "STAMP"; }
        }

        public override int ZIndex
        {
            get { return 60; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            base.OnAdded(fromHistory);

            if (!fromHistory)
            {
                ApplyDistinctPosition(this);

                return;
            }

            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var strokesToRestore = new StrokeCollection();

            foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.History.TrashedInkStrokes.Contains(stroke)))
            {
                strokesToRestore.Add(stroke);
            }

            ParentPage.InkStrokes.Add(strokesToRestore);
            ParentPage.History.TrashedInkStrokes.Remove(strokesToRestore);
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            base.OnDeleted(fromHistory);

            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var strokesToTrash = new StrokeCollection();

            foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.InkStrokes.Contains(stroke)))
            {
                strokesToTrash.Add(stroke);
            }

            ParentPage.InkStrokes.Remove(strokesToTrash);
            ParentPage.History.TrashedInkStrokes.Add(strokesToTrash);
        }

        public override void OnMoving(double oldX, double oldY, bool fromHistory = false)
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

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            base.OnMoved(oldX, oldY, fromHistory);

            OnMoving(oldX, oldY, fromHistory);
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

        public override string GetCodedIDAtHistoryIndex(int historyIndex)
        {
            var parts = GetPartsAtHistoryIndex(historyIndex);
            return parts <= 0 ? "blank" : parts.ToString();
        }

        #endregion //APageObjectBase Overrides

        #region AStrokeAccepter Overrides

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public override int StrokeHitTestPercentage
        {
            get { return 50; }
        }

        public override Rect StrokeAcceptanceBoundingBox
        {
            get { return new Rect(XPosition, YPosition + HandleHeight, Width, Height - HandleHeight - PartsHeight); }
        }

        #endregion //AStrokeAccepter Overrides

        #region APageObjectAccepter Overrides

        public override void ChangeAcceptedPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            base.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);

            RefreshParts();
        }

        public override bool IsPageObjectTypeAcceptedByThisPageObject(IPageObject pageObject)
        {
            var stampedObject = pageObject as StampedObject;
            if (stampedObject == null)
            {
                return false;
            }

            return stampedObject.StampedObjectType == StampedObjectTypes.GeneralStampedObject;
        }

        #endregion //APageObjectAccepter Overrides

        #region ICountable Implementation

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

        public void RefreshParts()
        {
            Parts = 0;
            foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
            {
                Parts += pageObject.Parts;
            }
        }

        #endregion //ICountable Implementation

        #region IReporter Implementation

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
                                     (x.StampedObjectType == StampedObjectTypes.GroupStampedObject || x.StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject) &&
                                     x.Parts > 0)
                              .ToList();

                var groupedStampedObjects = childStampedObjects.Where(c => groupStampedObjects.Count(x => x.AcceptedPageObjectIDs.Contains(c.ID)) > 0).ToList();

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
                                     (x.StampedObjectType == StampedObjectTypes.GroupStampedObject || x.StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject) &&
                                     x.Parts > 0)
                              .ToList();

                var groupedStampedObjects = childStampedObjects.Where(c => groupStampedObjects.Count(x => x.AcceptedPageObjectIDs.Contains(c.ID)) > 0).ToList();

                return childStampedObjects.Count - groupedStampedObjects.Count;
            }
        }

        public string FormattedReport
        {
            get { return StampType != StampTypes.ObservingStamp ? string.Empty : string.Format("{0} in groups\n" + "{1} not in groups", NumberInGroups, NumberNotInGroups); }
        }

        public void UpdateReport() { RaisePropertyChanged("FormattedReport"); }

        #endregion //IReporter Implementation
    }
}
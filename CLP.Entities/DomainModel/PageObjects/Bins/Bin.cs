using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Collections;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class Bin : APageObjectBase, ICountable, IPageObjectAccepter, IStrokeAccepter
    {
        #region Constructors

        /// <summary>Initializes <see cref="Bin" /> from scratch.</summary>
        public Bin() { }

        /// <summary>Initializes <see cref="Bin" /> from <see cref="ShapeType" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Bin" /> belongs to.</param>
        public Bin(CLPPage parentPage)
            : base(parentPage)
        {
            Height = 165 + PartsHeight;
            Width = 165;
        }

        /// <summary>Initializes <see cref="Bin" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Bin(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double PartsHeight
        {
            get { return 20; }
        }

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Bin of {0}", Parts); }
        }

        public override int ZIndex
        {
            get { return 70; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            if (CanAcceptStrokes &&
                AcceptedStrokes.Any() &&
                fromHistory)
            {
                var strokesToRestore = new StrokeCollection();

                foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.History.TrashedInkStrokes.Contains(stroke)))
                {
                    strokesToRestore.Add(stroke);
                }

                ParentPage.History.TrashedInkStrokes.Remove(strokesToRestore);
                ParentPage.InkStrokes.Add(strokesToRestore);
            }

            if (CanAcceptPageObjects &&
                AcceptedPageObjects.Any() &&
                fromHistory)
            {
                var pageObjectsToRestore = new List<IPageObject>();

                foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.History.TrashedPageObjects.Contains(p)))
                {
                    pageObjectsToRestore.Add(pageObject);
                }

                ParentPage.PageObjects.AddRange(pageObjectsToRestore);
                foreach (var pageObject in pageObjectsToRestore)
                {
                    ParentPage.History.TrashedPageObjects.Remove(pageObject);
                }
            }

            base.OnAdded(fromHistory);
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            if (CanAcceptStrokes &&
                AcceptedStrokes.Any())
            {
                var strokesToTrash = new StrokeCollection();

                foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.InkStrokes.Contains(stroke)))
                {
                    strokesToTrash.Add(stroke);
                }

                ParentPage.History.TrashedInkStrokes.Add(strokesToTrash);
                ParentPage.InkStrokes.Remove(strokesToTrash);
            }

            if (CanAcceptPageObjects && 
                AcceptedPageObjects.Any())
            {
                var pageObjectsToTrash = new List<IPageObject>();

                foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.PageObjects.Contains(p)))
                {
                    pageObjectsToTrash.Add(pageObject);
                }

                foreach (var pageObject in pageObjectsToTrash)
                {
                    ParentPage.PageObjects.Remove(pageObject);
                }

                ParentPage.History.TrashedPageObjects.AddRange(pageObjectsToTrash);
            }

            base.OnDeleted(fromHistory);
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

            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in AcceptedPageObjects)
            {
                pageObject.XPosition += deltaX;
                pageObject.YPosition += deltaY;
            }
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            OnMoving(oldX, oldY, fromHistory);
            base.OnMoved(oldX, oldY, fromHistory);
        }

        public override IPageObject Duplicate()
        {
            var newBin = Clone() as Bin;
            if (newBin == null)
            {
                return null;
            }
            newBin.CreationDate = DateTime.Now;
            newBin.ID = Guid.NewGuid().ToCompactID();
            newBin.VersionIndex = 0;
            newBin.LastVersionIndex = null;
            newBin.ParentPage = ParentPage;

            return newBin;
        }

        #endregion //APageObjectBase Overrides

        #region ICountable Implementation

        /// <summary>Number of parts the <see cref="Bin" /> represents.</summary>
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

        public static readonly PropertyData IsPartsAutoGeneratedProperty = RegisterProperty("IsPartsAutoGenerated", typeof (bool), true);

        public void RefreshParts()
        {
            Parts = 0;
            if (CanAcceptPageObjects)
            {
                foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
                {
                    Parts += pageObject.Parts;
                }
            }

            if (CanAcceptStrokes)
            {
                Parts += AcceptedStrokes.Count;
            }
        }

        #endregion //ICountable Implementation

        #region IPageObjectAccepter Implementation

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="IPageObject" />s.</summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof (bool), true);

        /// <summary>The currently accepted <see cref="IPageObject" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<IPageObject> AcceptedPageObjects
        {
            get { return GetValue<List<IPageObject>>(AcceptedPageObjectsProperty); }
            set { SetValue(AcceptedPageObjectsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectsProperty = RegisterProperty("AcceptedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        /// <summary>The IDs of the <see cref="IPageObject" />s that have been accepted.</summary>
        public List<string> AcceptedPageObjectIDs
        {
            get { return GetValue<List<string>>(AcceptedPageObjectIDsProperty); }
            set { SetValue(AcceptedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectIDsProperty = RegisterProperty("AcceptedPageObjectIDs", typeof (List<string>), () => new List<string>());

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

            foreach (var pageObject in addedPageObjects.OfType<Mark>())
            {
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
                                       where PageObjectIsOver(pageObject, .90) //PageObject must be at least 90% contained by Bin.
                                       select pageObject;

            AcceptPageObjects(pageObjectsOverStamp, new List<IPageObject>());
        }

        #endregion //IPageObjectAccepter Implementation

        #region IStrokeAccepter Implementation

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public int StrokeHitTestPercentage
        {
            get { return 90; }
        }

        /// <summary>Determines whether the <see cref="Bin" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes", typeof(List<Stroke>), () => new List<Stroke>());

        /// <summary>The IDs of the <see cref="Stroke" />s that have been accepted.</summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs", typeof(List<string>), () => new List<string>());

        public void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            // Remove Strokes
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            foreach (var stroke in removedStrokesList.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
            }

            // Add Strokes
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            foreach (var stroke in addedStrokesList.Where(stroke => IsStrokeOverPageObject(stroke) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }

            RefreshParts();
            ParentPage.UpdateAllReporters();
        }

        public bool IsStrokeOverPageObject(Stroke stroke)
        {
            var binBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            return stroke.HitTest(binBoundingBox, StrokeHitTestPercentage);
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var binBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(binBoundingBox, StrokeHitTestPercentage)
                                    select stroke;

            return new StrokeCollection(strokesOverObject);
        }

        public void RefreshAcceptedStrokes()
        {
            AcceptedStrokes.Clear();
            AcceptedStrokeParentIDs.Clear();
            if (!CanAcceptStrokes)
            {
                return;
            }

            var strokesOverObject = GetStrokesOverPageObject();

            ChangeAcceptedStrokes(strokesOverObject, new StrokeCollection());
        }

        #endregion //IStrokeAccepter Implementation
    }
}
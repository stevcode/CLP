using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Collections;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class Bin : AStrokeAndPageObjectAccepter, ICountable
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

        public override string CodedName
        {
            get { return Codings.OBJECT_BINS; }
        }

        public override string CodedID
        {
            get { return "A"; }
        }

        public override int ZIndex
        {
            get { return 70; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        /// <summary>Minimum Height of the <see cref="IPageObject" />.</summary>
        public override double MinimumHeight
        {
            get { return 35 + PartsHeight; }
        }

        /// <summary>Minimum Width of the <see cref="IPageObject" />.</summary>
        public override double MinimumWidth
        {
            get { return 100; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            if (CanAcceptStrokes &&
                AcceptedStrokes.Any() &&
                !fromHistory)
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
                !fromHistory)
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
            if (CanAcceptStrokes && AcceptedStrokes.Any())
            {
                var strokesToTrash = new StrokeCollection();

                foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.InkStrokes.Contains(stroke)))
                {
                    strokesToTrash.Add(stroke);
                }

                ParentPage.History.TrashedInkStrokes.Add(strokesToTrash);
                ParentPage.InkStrokes.Remove(strokesToTrash);
            }

            if (CanAcceptPageObjects && AcceptedPageObjects.Any())
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

        public override void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var scaleX = Width / oldWidth;
            var scaleY = Height / oldHeight;

            AcceptedStrokes.StretchAll(scaleX, scaleY, XPosition, YPosition);
        }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            base.OnResized(oldWidth, oldHeight, fromHistory);

            OnResizing(oldWidth, oldHeight, fromHistory);
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

        #region AStrokeAccepter Overrides

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public override int StrokeHitTestPercentage
        {
            get { return 90; }
        }

        public override void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            base.ChangeAcceptedStrokes(addedStrokes, removedStrokes);

            RefreshParts();
            ParentPage.UpdateAllReporters();
        }

        #endregion //AStrokeAccepter Overrides

        #region APageObjectAccepter Overrides

        public override void ChangeAcceptedPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            base.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);

            RefreshParts();
        }

        public override bool IsPageObjectTypeAcceptedByThisPageObject(IPageObject pageObject) { return pageObject is Mark; }

        #endregion //APageObjectAccepter Overrides

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
    }
}
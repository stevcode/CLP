using System;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    public abstract class APageObjectBase : AEntityBase, IPageObject
    {
        #region Constructors

        /// <summary>Initializes <see cref="APageObjectBase" /> from scratch.</summary>
        protected APageObjectBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="APageObjectBase" /> using <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IPageObject" /> belongs to.</param>
        protected APageObjectBase(CLPPage parentPage)
            : this()
        {
            ParentPage = parentPage;
            OwnerID = parentPage.OwnerID;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="IPageObject" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>X Position of the <see cref="IPageObject" />.</summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>Y Position of the <see cref="IPageObject" />.</summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 250.0);

        /// <summary>Height of the <see cref="IPageObject" />.</summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 40.0);

        /// <summary>Width of the <see cref="IPageObject" />.</summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 40.0);

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="IPageObject" />.</summary>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>Unique Identifier for the <see cref="Person" /> who created the <see cref="IPageObject" />.</summary>
        public string CreatorID
        {
            get { return GetValue<string>(CreatorIDProperty); }
            set { SetValue(CreatorIDProperty, value); }
        }

        public static readonly PropertyData CreatorIDProperty = RegisterProperty("CreatorID", typeof(string), string.Empty);

        /// <summary>Date and Time the <see cref="IPageObject" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Version marker to trigger different looks or calculated property values based on a previous, prototyped version of a <see cref="IPageObject" />.</summary>
        public string PageObjectFunctionalityVersion
        {
            get { return GetValue<string>(PageObjectFunctionalityVersionProperty); }
            set { SetValue(PageObjectFunctionalityVersionProperty, value); }
        }

        public static readonly PropertyData PageObjectFunctionalityVersionProperty = RegisterProperty("PageObjectFunctionalityVersion", typeof(string), "0");

        /// <summary>Determines whether the <see cref="IPageObject" />'s properties can be changed by a <see cref="Person" /> other than the Creator.</summary>
        public bool IsManipulatableByNonCreator
        {
            get { return GetValue<bool>(IsManipulatableByNonCreatorProperty); }
            set { SetValue(IsManipulatableByNonCreatorProperty, value); }
        }

        public static readonly PropertyData IsManipulatableByNonCreatorProperty = RegisterProperty("IsManipulatableByNonCreator", typeof(bool), false);

        /// <summary>The <see cref="IPageObject" />'s parent <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #region Calculated Properties

        public virtual string FormattedName => GetType().Name;

        public virtual string CodedName => "Coded Name not implemented.";

        public virtual string CodedID => "CodedID not implemented.";

        public abstract int ZIndex { get; }

        /// <summary>Minimum Height of the <see cref="IPageObject" />.</summary>
        public virtual double MinimumHeight => 20;

        /// <summary>Minimum Width of the <see cref="IPageObject" />.</summary>
        public virtual double MinimumWidth => 20;

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public abstract bool IsBackgroundInteractable { get; }

        #endregion // Calculated Properties

        #endregion //Properties

        #region Methods

        public virtual IPageObject Duplicate()
        {
            IPageObject pageObject = DeepCopy();
            if (pageObject == null)
            {
                return null;
            }

            pageObject.CreationDate = DateTime.Now;
            pageObject.ID = Guid.NewGuid().ToCompactID();
            pageObject.ParentPage = ParentPage;

            return pageObject;
        }

        public virtual void OnAdded(bool fromHistory = false)
        {
            ParentPage.UpdateAllReporters();
        }

        public virtual void OnDeleted(bool fromHistory = false)
        {
            ParentPage.UpdateAllReporters();
        }

        public virtual void OnMoving(double oldX, double oldY, bool fromHistory = false) { }

        public virtual void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            ParentPage.UpdateAllReporters();
        }

        public virtual void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false) { }

        public virtual void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            ParentPage.UpdateAllReporters();
        }

        public virtual void OnRotating(double oldAngle, bool fromHistory = false) { }

        public virtual void OnRotated(double oldAngle, bool fromHistory = false)
        {
            ParentPage.UpdateAllReporters();
        }

        public virtual bool PageObjectIsOver(IPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var area = Height * Width;
            var top = Math.Max(YPosition, pageObject.YPosition);
            var bottom = Math.Min(YPosition + Height, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition, pageObject.XPosition);
            var right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && (intersectionArea / areaObject >= percentage || intersectionArea / area >= percentage);
        }

        #region History Methods

        /// <summary>Signifies the pageObject was on the page immediately after the historyItem at the given historyIndex was performed</summary>
        public virtual bool IsOnPageAtHistoryIndex(int historyIndex)
        {
            var orderedObjectsOnPageChangedHistoryItems = ParentPage.History.CompleteOrderedHistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().ToList();
            var addedAtAnyPointHistoryItem = orderedObjectsOnPageChangedHistoryItems.FirstOrDefault(h => h.PageObjectIDsAdded.Contains(ID));
            var isPartOfHistory = addedAtAnyPointHistoryItem != null;

            var addedOrRemovedBeforeThisHistoryIndexHistoryItem =
                orderedObjectsOnPageChangedHistoryItems.LastOrDefault(h => (h.PageObjectIDsAdded.Contains(ID) || h.PageObjectIDsRemoved.Contains(ID)) && h.HistoryIndex <= historyIndex);

            var isOnPageBefore = addedOrRemovedBeforeThisHistoryIndexHistoryItem != null && addedOrRemovedBeforeThisHistoryIndexHistoryItem.PageObjectIDsAdded.Contains(ID);

            return isOnPageBefore || !isPartOfHistory;
        }

        /// <summary>Gets CodedID just before the historyItem at historyIndex executes Redo(). To get CodedID just after historyItem executes Redo(), add 1 to historyIndex.</summary>
        public virtual string GetCodedIDAtHistoryIndex(int historyIndex)
        {
            return CodedID;
        }

        /// <summary>Gets a new Point(Width, Height) just before the historyItem at historyIndex executes Redo(). To get (Width, Height) just after historyItem executes Redo(), add 1 to historyIndex.</summary>
        public virtual Point GetDimensionsAtHistoryIndex(int historyIndex)
        {
            var resizeHistoryItem = ParentPage.History.CompleteOrderedHistoryItems.OfType<PageObjectResizeBatchHistoryItem>()
                                              .FirstOrDefault(h => h.PageObjectID == ID && h.HistoryIndex >= historyIndex);
            if (resizeHistoryItem == null ||
                !resizeHistoryItem.StretchedDimensions.Any())
            {
                return new Point(Width, Height);
            }

            return resizeHistoryItem.StretchedDimensions.First();

            // TODO: numberline.endpointchange, remainderTiles.updated
        }

        /// <summary>Gets a new Point(XPos, YPos) just before the historyItem at historyIndex executes Redo(). To get (XPos, YPos) just after historyItem executes Redo(), add 1 to historyIndex.</summary>
        public virtual Point GetPositionAtHistoryIndex(int historyIndex)
        {
            var moveHistoryItem =
                ParentPage.History.CompleteOrderedHistoryItems.OfType<ObjectsMovedBatchHistoryItem>().FirstOrDefault(h => h.PageObjectIDs.ContainsKey(ID) && h.HistoryIndex >= historyIndex);

            if (moveHistoryItem == null ||
                !moveHistoryItem.TravelledPositions.Any())
            {
                return new Point(XPosition, YPosition);
            }

            var initialPosition = moveHistoryItem.TravelledPositions.First();
            var offset = moveHistoryItem.PageObjectIDs[ID];
            var adjustedPosition = new Point(initialPosition.X + offset.X, initialPosition.Y + offset.Y);
            return adjustedPosition;
        }

        /// <summary>
        ///     Gets a new Point(XPos, YPos) just before the historyItem at historyIndex executes Redo(). To get (XPos, YPos) just after historyItem executes Redo(), add 1 to historyIndex. // TODO: Modify
        ///     these to be after current historyItems executes Redo().
        /// </summary>
        public virtual Rect GetBoundsAtHistoryIndex(int historyIndex)
        {
            var position = GetPositionAtHistoryIndex(historyIndex);
            var dimensions = GetDimensionsAtHistoryIndex(historyIndex);
            return new Rect(position.X, position.Y, dimensions.X, dimensions.Y);
        }

        #endregion //History Methods

        #endregion //Methods

        #region Utility Methods

        public static void ApplyDistinctPosition(IPageObject placedPageObject)
        {
            var isAtHorizontalEdge = false;
            var isAtVerticalEdge = false;

            foreach (var pageObject in placedPageObject.ParentPage.PageObjects)
            {
                if (pageObject.GetType() != placedPageObject.GetType() ||
                    pageObject.ID == placedPageObject.ID)
                {
                    continue;
                }

                var xDelta = Math.Abs(pageObject.XPosition - placedPageObject.XPosition);
                var yDelta = Math.Abs(pageObject.YPosition - placedPageObject.YPosition);
                if (placedPageObject is NumberLine)
                {
                    yDelta = Math.Abs((pageObject.YPosition + pageObject.Height) - (placedPageObject.YPosition + placedPageObject.Height));
                }

                if (xDelta > 20 ||
                    yDelta > 20)
                {
                    continue;
                }

                if (xDelta < 20)
                {
                    if (placedPageObject.XPosition + 21 + placedPageObject.Width < placedPageObject.ParentPage.Width)
                    {
                        placedPageObject.XPosition += 21;
                    }
                    else
                    {
                        placedPageObject.XPosition = placedPageObject.ParentPage.Width - placedPageObject.Width;
                        isAtHorizontalEdge = true;
                    }
                }

                if (yDelta < 20)
                {
                    if (placedPageObject.YPosition + 21 + placedPageObject.Height < placedPageObject.ParentPage.Height)
                    {
                        placedPageObject.YPosition += 21;
                    }
                    else
                    {
                        placedPageObject.YPosition = placedPageObject.ParentPage.Height - placedPageObject.Height;
                        isAtVerticalEdge = true;
                    }
                }

                if (!isAtHorizontalEdge &&
                    !isAtVerticalEdge)
                {
                    ApplyDistinctPosition(placedPageObject);
                }
            }

            if (placedPageObject.XPosition + placedPageObject.Width >= placedPageObject.ParentPage.Width)
            {
                placedPageObject.XPosition = placedPageObject.ParentPage.Width - placedPageObject.Width;
            }
            if (placedPageObject.YPosition + placedPageObject.Height >= placedPageObject.ParentPage.Height)
            {
                placedPageObject.YPosition = placedPageObject.ParentPage.Height - placedPageObject.Height;
            }
        }

        public static bool IsPageObjectAnAcceptedPageObject(IPageObject pageObject)
        {
            return pageObject.ParentPage.PageObjects.OfType<IPageObjectAccepter>().Any(x => x.AcceptedPageObjectIDs.Contains(pageObject.ID));
        }

        public static bool IsOverlapping(IPageObject firstPageObject, IPageObject secondPageObject)
        {
            var firstBounds = new Rect(firstPageObject.XPosition, firstPageObject.YPosition, firstPageObject.Width, firstPageObject.Height);
            var secondBounds = new Rect(secondPageObject.XPosition, secondPageObject.YPosition, secondPageObject.Width, secondPageObject.Height);
            return IsBoundsOverlapping(firstBounds, secondBounds);
        }

        public static bool IsBoundsOverlapping(Rect firstBounds, Rect secondBounds)
        {
            return firstBounds.IntersectsWith(secondBounds);
        }

        public static bool IsBoundsOverlappingByPercentage(Rect firstBounds, Rect secondBounds, double percentage)
        {
            var intersectRect = Rect.Intersect(firstBounds, secondBounds);
            return intersectRect.Area() / secondBounds.Area() >= percentage;
        }

        #endregion //Utility Methods
    }
}
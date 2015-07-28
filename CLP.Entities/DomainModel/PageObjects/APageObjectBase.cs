using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;

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

        /// <summary>Initializes <see cref="APageObjectBase" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public APageObjectBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public virtual string FormattedName
        {
            get { return GetType().Name; }
        }

        /// <summary>Unique Identifier for the <see cref="IPageObject" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="IPageObject" />.</summary>
        /// <remarks>Composite Primary Key. Also Foregin Key for <see cref="Person" /> who owns the <see cref="IPageObject" />.</remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof (string), string.Empty);

        /// <summary>Version Index of the <see cref="IPageObject" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof (uint), 0);

        /// <summary>Version Index of the latest submission.</summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof (uint?));

        /// <summary>Differentiation Level of the <see cref="IPageObject" />.</summary>
        public string DifferentiationLevel
        {
            get { return GetValue<string>(DifferentiationLevelProperty); }
            set { SetValue(DifferentiationLevelProperty, value); }
        }

        public static readonly PropertyData DifferentiationLevelProperty = RegisterProperty("DifferentiationLevel", typeof (string), "0");

        /// <summary>Version marker to trigger different looks or calculated property values based on a previous, prototyped version of a <see cref="IPageObject" />.</summary>
        public string PageObjectFunctionalityVersion
        {
            get { return GetValue<string>(PageObjectFunctionalityVersionProperty); }
            set { SetValue(PageObjectFunctionalityVersionProperty, value); }
        }

        public static readonly PropertyData PageObjectFunctionalityVersionProperty = RegisterProperty("PageObjectFunctionalityVersion", typeof(string), "0");

        /// <summary>Date and Time the <see cref="IPageObject" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof (DateTime));

        /// <summary>X Position of the <see cref="IPageObject" />.</summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof (double), 10.0);

        /// <summary>Y Position of the <see cref="IPageObject" />.</summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof (double), 250.0);

        public abstract int ZIndex { get; }

        /// <summary>Height of the <see cref="IPageObject" />.</summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof (double), 40.0);

        /// <summary>Width of the <see cref="IPageObject" />.</summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof (double), 40.0);

        /// <summary>Minimum Height of the <see cref="IPageObject" />.</summary>
        public virtual double MinimumHeight
        {
            get { return 10; }
        }

        /// <summary>Minimum Width of the <see cref="IPageObject" />.</summary>
        public virtual double MinimumWidth
        {
            get { return 10; }
        }

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public abstract bool IsBackgroundInteractable { get; }

        /// <summary>Determines whether the <see cref="IPageObject" />'s properties can be changed by a <see cref="Person" /> other than the Creator.</summary>
        public bool IsManipulatableByNonCreator
        {
            get { return GetValue<bool>(IsManipulatableByNonCreatorProperty); }
            set { SetValue(IsManipulatableByNonCreatorProperty, value); }
        }

        public static readonly PropertyData IsManipulatableByNonCreatorProperty = RegisterProperty("IsManipulatableByNonCreator", typeof (bool), false);

        /// <summary>Unique Identifier for the <see cref="Person" /> who created the <see cref="IPageObject" />.</summary>
        public string CreatorID
        {
            get { return GetValue<string>(CreatorIDProperty); }
            set { SetValue(CreatorIDProperty, value); }
        }

        public static readonly PropertyData CreatorIDProperty = RegisterProperty("CreatorID", typeof (string), string.Empty);

        #region Navigation Properties

        /// <summary>Unique Identifier for the <see cref="IPageObject" />'s parent <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Foreign Key.</remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof (string));

        /// <summary>Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="IPageObject" />.</summary>
        /// <remarks>Composite Foreign Key.</remarks>
        public string ParentPageOwnerID
        {
            get { return GetValue<string>(ParentPageOwnerIDProperty); }
            set { SetValue(ParentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData ParentPageOwnerIDProperty = RegisterProperty("ParentPageOwnerID", typeof (string));

        /// <summary>The parent <see cref="CLPPage" />'s Version Index.</summary>
        public uint ParentPageVersionIndex
        {
            get { return GetValue<uint>(ParentPageVersionIndexProperty); }
            set { SetValue(ParentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData ParentPageVersionIndexProperty = RegisterProperty("ParentPageVersionIndex", typeof (uint), 0);

        /// <summary>The <see cref="IPageObject" />'s parent <see cref="CLPPage" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
                if (value == null)
                {
                    return;
                }
                ParentPageID = value.ID;
                ParentPageOwnerID = value.OwnerID;
                ParentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof (CLPPage));

        #endregion //Navigation Properties

        #endregion //Properties

        #region Methods

        public abstract IPageObject Duplicate();

        public virtual void OnAdded(bool fromHistory = false) { ParentPage.UpdateAllReporters(); }

        public virtual void OnDeleted(bool fromHistory = false) { ParentPage.UpdateAllReporters(); }

        public virtual void OnMoving(double oldX, double oldY, bool fromHistory = false) { }

        public virtual void OnMoved(double oldX, double oldY, bool fromHistory = false) { ParentPage.UpdateAllReporters(); }

        public virtual void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false) { }

        public virtual void OnResized(double oldWidth, double oldHeight, bool fromHistory = false) { ParentPage.UpdateAllReporters(); }

        public virtual void OnRotating(double oldAngle, bool fromHistory = false) { }

        public virtual void OnRotated(double oldAngle, bool fromHistory = false) { ParentPage.UpdateAllReporters(); }

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
            return firstBounds.IntersectsWith(secondBounds);
        }

        #endregion //Utility Methods
    }
}
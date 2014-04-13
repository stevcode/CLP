using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class APageObjectBase : AEntityBase, IPageObject
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="APageObjectBase" /> from scratch.
        /// </summary>
        protected APageObjectBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes <see cref="APageObjectBase" /> using <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IPageObject" /> belongs to.</param>
        protected APageObjectBase(CLPPage parentPage)
            : this() { ParentPage = parentPage; }

        /// <summary>
        /// Initializes <see cref="APageObjectBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public APageObjectBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="IPageObject" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="IPageObject" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// Also Foregin Key for <see cref="Person" /> who owns the <see cref="IPageObject" />.
        /// </remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>
        /// Version Index of the <see cref="IPageObject" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint), 0);

        /// <summary>
        /// Version Index of the latest submission.
        /// </summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof(uint?));

        /// <summary>
        /// Date and Time the <see cref="IPageObject" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// X Position of the <see cref="IPageObject" />.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// Y Position of the <see cref="IPageObject" />.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 250.0);

        /// <summary>
        /// Height of the <see cref="IPageObject" />.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 20.0);

        /// <summary>
        /// Width of the <see cref="IPageObject" />.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 20.0);

        /// <summary>
        /// Minimum Height of the <see cref="IPageObject" />.
        /// </summary>
        public virtual double MinimumHeight
        {
            get { return 10; }
        }

        /// <summary>
        /// Minimum Width of the <see cref="IPageObject" />.
        /// </summary>
        public virtual double MinimumWidth
        {
            get { return 10; }
        }

        /// <summary>
        /// Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.
        /// </summary>
        public abstract bool IsBackgroundInteractable { get; }

        /// <summary>
        /// Determines whether the <see cref="IPageObject" />'s properties can be changed by a <see cref="Person" /> other than the Creator.
        /// </summary>
        public bool IsManipulatableByNonCreator
        {
            get { return GetValue<bool>(IsManipulatableByNonCreatorProperty); }
            set { SetValue(IsManipulatableByNonCreatorProperty, value); }
        }

        public static readonly PropertyData IsManipulatableByNonCreatorProperty = RegisterProperty("IsManipulatableByNonCreator", typeof(bool), false);

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who created the <see cref="IPageObject" />.
        /// </summary>
        public string CreatorID
        {
            get { return GetValue<string>(CreatorIDProperty); }
            set { SetValue(CreatorIDProperty, value); }
        }

        public static readonly PropertyData CreatorIDProperty = RegisterProperty("CreatorID", typeof(string), string.Empty);

        #region Navigation Properties

        /// <summary>
        /// Unique Identifier for the <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string));

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="IPageObject" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        public string ParentPageOwnerID
        {
            get { return GetValue<string>(ParentPageOwnerIDProperty); }
            set { SetValue(ParentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData ParentPageOwnerIDProperty = RegisterProperty("ParentPageOwnerID", typeof(string));

        /// <summary>
        /// The parent <see cref="CLPPage" />'s Version Index.
        /// </summary>
        public uint ParentPageVersionIndex
        {
            get { return GetValue<uint>(ParentPageVersionIndexProperty); }
            set { SetValue(ParentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData ParentPageVersionIndexProperty = RegisterProperty("ParentPageVersionIndex", typeof(uint), 0);

        /// <summary>
        /// The <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
                if(value == null)
                {
                    return;
                }
                ParentPageID = value.ID;
                ParentPageOwnerID = value.OwnerID;
                ParentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #endregion //Navigation Properties

        #endregion //Properties

        #region Methods

        public abstract IPageObject Duplicate();

        public virtual void OnAdded() { }

        public virtual void OnDeleted() { }

        public virtual void OnMoved() { }

        public virtual void OnResized() { }

        #endregion //Methods

        #region Utility Methods

        public static void ApplyDistinctPosition(IPageObject placedPageObject)
        {
            var isAtHorizontalEdge = false;
            var isAtVerticalEdge = false;

            foreach(IPageObject pageObject in placedPageObject.ParentPage.PageObjects)
            {
                if(pageObject.GetType() != placedPageObject.GetType() ||
                   pageObject.ID == placedPageObject.ID)
                {
                    continue;
                }

                var xDelta = Math.Abs(pageObject.XPosition - placedPageObject.XPosition);
                var yDelta = Math.Abs(pageObject.YPosition - placedPageObject.YPosition);

                if(xDelta > 20 ||
                   yDelta > 20)
                {
                    continue;
                }

                if(xDelta < 20)
                {
                    if(placedPageObject.XPosition + 21 + placedPageObject.Width < placedPageObject.ParentPage.Width)
                    {
                        placedPageObject.XPosition += 21;
                    }
                    else
                    {
                        placedPageObject.XPosition = placedPageObject.ParentPage.Width - placedPageObject.Width;
                        isAtHorizontalEdge = true;
                    }
                }

                if(yDelta < 20)
                {
                    if(placedPageObject.YPosition + 21 + placedPageObject.Height < placedPageObject.ParentPage.Height)
                    {
                        placedPageObject.YPosition += 21;
                    }
                    else
                    {
                        placedPageObject.YPosition = placedPageObject.ParentPage.Height - placedPageObject.Height;
                        isAtVerticalEdge = true;
                    }
                }

                if(!isAtHorizontalEdge &&
                   !isAtVerticalEdge)
                {
                    ApplyDistinctPosition(placedPageObject);
                }
            }
        }

        #endregion //Utility Methods
    }
}
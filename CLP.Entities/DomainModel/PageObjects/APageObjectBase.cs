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
        public APageObjectBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes <see cref="APageObjectBase" /> using <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IPageObject" /> belongs to.</param>
        public APageObjectBase(CLPPage parentPage)
            : this()
        {
            ParentPage = parentPage;
            ParentPageID = parentPage.ID;
        }

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

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 10.0);

        /// <summary>
        /// Width of the <see cref="IPageObject" />.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 10.0);

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
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string CreatorID
        {
            get { return GetValue<string>(CreatorIDProperty); }
            set { SetValue(CreatorIDProperty, value); }
        }

        public static readonly PropertyData CreatorIDProperty = RegisterProperty("CreatorID", typeof(string), string.Empty);

        /// <summary>
        /// The <see cref="Person" /> who created the <see cref="IPageObject" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual Person Creator
        {
            get { return GetValue<Person>(CreatorProperty); }
            set { SetValue(CreatorProperty, value); }
        }

        public static readonly PropertyData CreatorProperty = RegisterProperty("Creator", typeof(Person));

        /// <summary>
        /// Unique Identifier for the <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string), string.Empty);

        /// <summary>
        /// The <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

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
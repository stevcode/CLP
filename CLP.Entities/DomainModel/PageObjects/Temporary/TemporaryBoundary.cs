using Catel.Data;

namespace CLP.Entities
{
    public class TemporaryBoundary : APageObjectBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TemporaryBoundary" /> from scratch.</summary>
        public TemporaryBoundary() { }

        /// <summary>Initializes <see cref="TemporaryBoundary" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="LassoRegion" /> belongs to.</param>
        public TemporaryBoundary(CLPPage parentPage, double xPosition, double yPosition, double height, double width)
            : base(parentPage)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
        }

        #endregion //Constructors

        #region Properties

        public string RegionText
        {
            get { return GetValue<string>(RegionTextProperty); }
            set { SetValue(RegionTextProperty, value); }
        }

        public static readonly PropertyData RegionTextProperty = RegisterProperty("RegionText", typeof(string), string.Empty);

        #endregion // Properties

        #region APageObjectBase Overrides

        public override int ZIndex
        {
            get { return 1000; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        #endregion //Methods

        #region Static Methods

        public static void AddTemporaryBoundaryToPage(CLPPage page, double xPos, double yPos, double height, double width, string regionText = "")
        {
            if (page == null)
            {
                return;
            }

            var boundary = new TemporaryBoundary(page, xPos, yPos, height, width)
                           {
                               RegionText = regionText
                           };

            page.PageObjects.Add(boundary);
        }

        #endregion // Static Methods
    }
}
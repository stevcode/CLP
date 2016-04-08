using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class TemporaryGrid : APageObjectBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TemporaryGrid" /> from scratch.</summary>
        public TemporaryGrid() { }

        /// <summary>Initializes <see cref="TemporaryBoundary" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TemporaryGrid" /> belongs to.</param>
        public TemporaryGrid(CLPPage parentPage, double xPosition, double yPosition, double height, double width)
            : base(parentPage)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
        }

        /// <summary>Initializes <see cref="TemporaryBoundary" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TemporaryGrid(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

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

        public override IPageObject Duplicate()
        {
            var newGrid = Clone() as TemporaryGrid;
            if (newGrid == null)
            {
                return null;
            }
            newGrid.CreationDate = DateTime.Now;
            newGrid.ID = Guid.NewGuid().ToString();
            newGrid.VersionIndex = 0;
            newGrid.LastVersionIndex = null;
            newGrid.ParentPage = ParentPage;

            return newGrid;
        }

        #endregion //Methods
    }
}
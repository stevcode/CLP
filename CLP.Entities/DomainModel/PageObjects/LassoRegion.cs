using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class LassoRegion : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> from scratch.
        /// </summary>
        public LassoRegion() { }

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="LassoRegion" /> belongs to.</param>
        public LassoRegion(CLPPage parentPage, List<string> pageObjectIDs, List<string> inkStrokeIDs, double xPosition, double yPosition, double height, double width)
            : base(parentPage)
        {
            ContainedPageObjectIDs = pageObjectIDs;
            ContainedInkStrokeIDs = inkStrokeIDs;
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
        }

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public LassoRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        /// <summary>
        /// List of all the IDs of the <see cref="IPageObject" />s inside the <see cref="LassoRegion" />.
        /// </summary>
        public List<string> ContainedPageObjectIDs
        {
            get { return GetValue<List<string>>(ContainedPageObjectIDsProperty); }
            set { SetValue(ContainedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData ContainedPageObjectIDsProperty = RegisterProperty("ContainedPageObjectIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// List of all the IDs of the <see cref="StrokeDTO" />s inside the <see cref="LassoRegion" />.
        /// </summary>
        public List<string> ContainedInkStrokeIDs
        {
            get { return GetValue<List<string>>(ContainedInkStrokeIDsProperty); }
            set { SetValue(ContainedInkStrokeIDsProperty, value); }
        }

        public static readonly PropertyData ContainedInkStrokeIDsProperty = RegisterProperty("ContainedInkStrokeIDs", typeof(List<string>), () => new List<string>());

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newLassoRegion = Clone() as LassoRegion;
            if(newLassoRegion == null)
            {
                return null;
            }
            newLassoRegion.CreationDate = DateTime.Now;
            newLassoRegion.ID = Guid.NewGuid().ToString();
            newLassoRegion.VersionIndex = 0;
            newLassoRegion.LastVersionIndex = null;
            newLassoRegion.ParentPage = ParentPage;

            return newLassoRegion;
        }

        #endregion //Methods
    }
}
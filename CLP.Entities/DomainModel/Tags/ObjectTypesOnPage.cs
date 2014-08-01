using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    public class ObjectTypesOnPage : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> from scratch.
        /// </summary>
        public ObjectTypesOnPage() { }

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> from values.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPage" /> belongs to.</param>
        public ObjectTypesOnPage(CLPPage parentPage, Origin origin, string currentUserID)
            : base(parentPage)
        {
            Origin = origin;
            IsSingleValueTag = true;
            ObjectTypes = parentPage.PageObjects.Where(pageObject => pageObject.OwnerID == currentUserID).Select(pageObject => pageObject.GetType()).Distinct().ToList();
            if(parentPage.InkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == currentUserID))
            {
                ObjectTypes.Add(typeof(Stroke));
            }
        }

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ObjectTypesOnPage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// List of all the <see cref="Type" />s of the relevant <see cref="IPageObject" />s on the <see cref="CLPPage" /> as well as Ink.
        /// </summary>
        public List<Type> ObjectTypes
        {
            get { return GetValue<List<Type>>(ObjectTypesProperty); }
            set { SetValue(ObjectTypesProperty, value); }
        }

        public static readonly PropertyData ObjectTypesProperty = RegisterProperty("ObjectTypes", typeof(List<Type>), () => new List<Type>());

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Object Types on Page: {0}", string.Join(", ", ObjectTypes)); }
        }

        #endregion //Properties
    }
}
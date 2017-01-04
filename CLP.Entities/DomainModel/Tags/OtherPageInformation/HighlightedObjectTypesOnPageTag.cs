using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class HighlightedObjectTypesOnPageTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="HighlightedObjectTypesOnPageTag" /> from scratch.
        /// </summary>
        public HighlightedObjectTypesOnPageTag() { }

        /// <summary>
        /// Initializes <see cref="HighlightedObjectTypesOnPageTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="HighlightedObjectTypesOnPageTag" /> belongs to.</param>
        public HighlightedObjectTypesOnPageTag(CLPPage parentPage, Origin origin, List<string> objectTypes)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            ObjectTypes = objectTypes;
        }

        /// <summary>
        /// Initializes <see cref="HighlightedObjectTypesOnPageTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public HighlightedObjectTypesOnPageTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the <see cref="Type" />s of the relevant <see cref="IPageObject" />s on the <see cref="CLPPage" /> as well as Ink.</summary>
        public List<string> ObjectTypes
        {
            get { return GetValue<List<string>>(ObjectTypesProperty); }
            set { SetValue(ObjectTypesProperty, value); }
        }

        public static readonly PropertyData ObjectTypesProperty = RegisterProperty("ObjectTypes", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Representation; }
        }

        public override string FormattedName
        {
            get { return "Highlighted Objects On Page"; }
        }

        public override string FormattedValue
        {
            get { return string.Join(",", ObjectTypes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
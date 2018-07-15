using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class HighlightedObjectTypesOnPageTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="HighlightedObjectTypesOnPageTag" /> from scratch.</summary>
        public HighlightedObjectTypesOnPageTag() { }

        /// <summary>Initializes <see cref="HighlightedObjectTypesOnPageTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="HighlightedObjectTypesOnPageTag" /> belongs to.</param>
        public HighlightedObjectTypesOnPageTag(CLPPage parentPage, Origin origin, List<string> objectTypes)
            : base(parentPage, origin)
        {
            ObjectTypes = objectTypes;
        }

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

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Representation;

        public override string FormattedName => "Highlighted Objects On Page";

        public override string FormattedValue
        {
            get { return string.Join(",", ObjectTypes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
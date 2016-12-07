using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class MetaDataTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="MetaDataTag" /> from scratch.</summary>
        public MetaDataTag() { }

        /// <summary>Initializes <see cref="MetaDataTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="MetaDataTag" /> belongs to.</param>
        /// <param name="origin"></param>
        public MetaDataTag(CLPPage parentPage, Origin origin, string name, string contents)
            : base(parentPage, origin)
        {
            TagName = name;
            TagContents = contents;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Key of the KeyValue Pair that makes up this tag.</summary>
        public string TagName
        {
            get { return GetValue<string>(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }

        public static readonly PropertyData TagNameProperty = RegisterProperty("TagName", typeof(string), string.Empty);

        /// <summary>Value of the KeyValue Pair that makes up this tag.</summary>
        public string TagContents
        {
            get { return GetValue<string>(TagContentsProperty); }
            set { SetValue(TagContentsProperty, value); }
        }

        public static readonly PropertyData TagContentsProperty = RegisterProperty("TagContents", typeof(string), string.Empty);
        
        #endregion //Properties

        #region ATagBase Overrides

        public override bool IsSingleValueTag => false;

        public override Category Category => Category.MetaData;

        public override string FormattedName => TagName;

        public override string FormattedValue => TagContents;

        #endregion //ATagBase Overrides
    }
}
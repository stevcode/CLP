using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class MetaDataTag : ATagBase
    {
        #region Constants

        public const string NAME_WORD_PROBLEM = "Word Problem";
        public const string NAME_ONLY_TOP_PROBLEM = "Analyzing Only Top Problem";
        public const string NAME_DIFFICULTY_LEVEL = "Difficulty Level";
        public const string NAME_SPECIAL_INTEREST_GROUPS = "Special Interest Groups";
        public const string NAME_REQUIRED_REPRESENTATIONS = "Required Representations";

        public const string VALUE_TRUE = "True";
        public const string VALUE_FALSE = "False";

        public const string VALUE_DIFFICULTY_NONE = "None";
        public const string VALUE_DIFFICULTY_EASY = "Easy";
        public const string VALUE_DIFFICULTY_MEDIUM = "Medium";
        public const string VALUE_DIFFICULTY_HARD = "Hard";

        public const string VALUE_SPECIAL_INTEREST_GROUP_CE = "CE";
        public const string VALUE_SPECIAL_INTEREST_GROUP_ZERO = "Zero";
        public const string VALUE_SPECIAL_INTEREST_GROUP_SCAF = "Scaf";
        public const string VALUE_SPECIAL_INTEREST_GROUP_2PSF = "2PSF";
        public const string VALUE_SPECIAL_INTEREST_GROUP_2PSS = "2PSS";

        public const string VALUE_REQUIRED_REPRESENTATIONS_ARRAY = "Array";
        public const string VALUE_REQUIRED_REPRESENTATIONS_STAMP = "Stamp";
        public const string VALUE_REQUIRED_REPRESENTATIONS_NUMBER_LINE = "Number Line";
        public const string VALUE_REQUIRED_REPRESENTATIONS_ARRAY_OR_NUMBER_LINE = "Array Or Number Line";
        public const string VALUE_REQUIRED_REPRESENTATIONS_ARRAY_AND_STAMP = "Array And Stamp";

        #endregion // Constants

        #region Constructors

        /// <summary>Initializes <see cref="MetaDataTag" /> from scratch.</summary>
        public MetaDataTag() { }

        /// <summary>Initializes <see cref="MetaDataTag" /> from values.</summary>
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
            get => GetValue<string>(TagNameProperty);
            set => SetValue(TagNameProperty, value);
        }

        public static readonly PropertyData TagNameProperty = RegisterProperty("TagName", typeof(string), string.Empty);

        /// <summary>Value of the KeyValue Pair that makes up this tag.</summary>
        public string TagContents
        {
            get => GetValue<string>(TagContentsProperty);
            set => SetValue(TagContentsProperty, value);
        }

        public static readonly PropertyData TagContentsProperty = RegisterProperty("TagContents", typeof(string), string.Empty);

        #endregion //Properties

        #region ATagBase Overrides

        public override Category Category => Category.MetaData;

        public override string FormattedName => TagName;

        public override string FormattedValue => TagContents;

        #endregion //ATagBase Overrides
    }
}
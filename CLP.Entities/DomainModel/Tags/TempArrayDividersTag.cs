using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class TempArrayDividersTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="TempArrayDividersTag" /> from scratch.
        /// </summary>
        public TempArrayDividersTag() { }

        /// <summary>
        /// Initializes <see cref="TempArrayDividersTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TempArrayDividersTag" /> belongs to.</param>
        public TempArrayDividersTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="TempArrayDividersTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TempArrayDividersTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// SUMMARY
        /// </summary>
        public string ArrayName
        {
            get { return GetValue<string>(ArrayNameProperty); }
            set { SetValue(ArrayNameProperty, value); }
        }

        public static readonly PropertyData ArrayNameProperty = RegisterProperty("ArrayName", typeof (string), string.Empty);

        /// <summary>
        /// SUMMARY
        /// </summary>
        public List<int> HorizontalDividers
        {
            get { return GetValue<List<int>>(HorizontalDividersProperty); }
            set { SetValue(HorizontalDividersProperty, value); }
        }

        public static readonly PropertyData HorizontalDividersProperty = RegisterProperty("HorizontalDividers", typeof (List<int>), () => new List<int>());

        /// <summary>
        /// SUMMARY
        /// </summary>
        public List<int> VerticalDividers
        {
            get { return GetValue<List<int>>(VerticalDividersProperty); }
            set { SetValue(VerticalDividersProperty, value); }
        }

        public static readonly PropertyData VerticalDividersProperty = RegisterProperty("VerticalDividers", typeof (List<int>), () => new List<int>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Ink Dividers"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0} Array\n" + "Horizontal Values: {1}\n" + "Vertical Values: {2}", ArrayName, string.Join(",", HorizontalDividers), string.Join(",", VerticalDividers)); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
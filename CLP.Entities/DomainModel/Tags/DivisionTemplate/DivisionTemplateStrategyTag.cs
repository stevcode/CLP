using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateStrategyTag : ATagBase
    {
        public enum AcceptedValues
        {
            OneArray, // Only one array snapped in
            Repeated, // e.g. 28 / 4 -> 4 x 3 | 4 x 3 | 4 x 1
            EvenSplit, // e.g. 28 / 2 -> 2 x 14 | 2 x 14
            Other
        }

        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateStrategyTag" /> from scratch.</summary>
        public DivisionTemplateStrategyTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateStrategyTag" /> from <see cref="AcceptedValues" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DivisionTemplateStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public DivisionTemplateStrategyTag(CLPPage parentPage, Origin origin, AcceptedValues value, List<int> dividerValues)
            : base(parentPage, origin)
        {
            Value = value;
            DividerValues = dividerValues;
        }

        /// <summary>Initializes <see cref="DivisionTemplateStrategyTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public AcceptedValues Value
        {
            get { return GetValue<AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof (AcceptedValues));

        /// <summary>List of all divider values used to fill up the Division Template.</summary>
        public List<int> DividerValues
        {
            get { return GetValue<List<int>>(DividerValuesProperty); }
            set { SetValue(DividerValuesProperty, value); }
        }

        public static readonly PropertyData DividerValuesProperty = RegisterProperty("DividerValues", typeof (List<int>), () => new List<int>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}: {1}", Value, string.Join(",", DividerValues.Select(x => x == 0 ? "?" : x.ToString()).Take(DividerValues.Count - 1))); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
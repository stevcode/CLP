using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayRegionStrategyTag : ATagBase
    {
        public enum AcceptedValues
        {
            FriendlyPartialProducts,
            NoDividers,
            Other
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayRegionStrategyTag" /> from scratch.
        /// </summary>
        public ArrayRegionStrategyTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayRegionStrategyTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayRegionStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayRegionStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayRegionStrategyTag(CLPPage parentPage, Origin origin, AcceptedValues value, List<string> regionDimensions)
            : base(parentPage, origin)
        {
            Value = value;
            RegionDimensions = regionDimensions;
        }

        /// <summary>
        /// Initializes <see cref="ArrayRegionStrategyTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayRegionStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the Starred Tag.
        /// </summary>
        public AcceptedValues Value
        {
            get { return GetValue<AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(AcceptedValues));

        /// <summary>
        /// Dimensions of all the Regions in the Array.
        /// </summary>
        public List<string> RegionDimensions
        {
            get { return GetValue<List<string>>(RegionDimensionsProperty); }
            set { SetValue(RegionDimensionsProperty, value); }
        }

        public static readonly PropertyData RegionDimensionsProperty = RegisterProperty("RegionDimensions", typeof(List<string>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}:{1}", Value, string.Join(",", RegionDimensions)); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
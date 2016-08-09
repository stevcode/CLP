using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionToolStrategies
    {
        OneArray, // Only one array snapped in
        Repeated, // e.g. 28 / 4 -> 4 x 3 | 4 x 3 | 4 x 1
        EvenSplit, // e.g. 28 / 2 -> 2 x 14 | 2 x 14
        Other
    }

    [Serializable]
    public class DivisionToolStrategyTag : ADivisionToolBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolStrategyTag" /> from scratch.</summary>
        public DivisionToolStrategyTag() { }

        /// <summary>Initializes <see cref="DivisionToolStrategyTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolStrategyTag" /> belongs to.</param>
        public DivisionToolStrategyTag(CLPPage parentPage,
                                           Origin origin,
                                           string divisionToolID,
                                           double dividend,
                                           double divisor,
                                           DivisionToolStrategies strategy,
                                           List<int> dividerValues,
                                           int divisionToolNumber)
            : base(parentPage, origin, divisionToolID, dividend, divisor, divisionToolNumber)
        {
            Strategy = strategy;
            DividerValues = dividerValues;
        }

        /// <summary>Initializes <see cref="DivisionToolStrategyTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public DivisionToolStrategies Strategy
        {
            get { return GetValue<DivisionToolStrategies>(StrategyProperty); }
            set { SetValue(StrategyProperty, value); }
        }

        public static readonly PropertyData StrategyProperty = RegisterProperty("Strategy", typeof (DivisionToolStrategies));

        /// <summary>List of all divider values used to fill up the Division Template.</summary>
        public List<int> DividerValues
        {
            get { return GetValue<List<int>>(DividerValuesProperty); }
            set { SetValue(DividerValuesProperty, value); }
        }

        public static readonly PropertyData DividerValuesProperty = RegisterProperty("DividerValues", typeof (List<int>), () => new List<int>());

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Tool {0} Strategy", DivisionToolNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Strategy for {0} / {1}\n" + "DivisionTool {2} on page.\n" + "{3}: {4}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionToolStillOnPage ? "still" : "no longer",
                                     Strategy,
                                     string.Join(",", DividerValues.Select(x => x == 0 ? "?" : x.ToString()).Take(DividerValues.Count - 1)));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
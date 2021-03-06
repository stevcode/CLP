﻿using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionTemplateStrategies
    {
        OneArray, // Only one array snapped in
        Repeated, // e.g. 28 / 4 -> 4 x 3 | 4 x 3 | 4 x 1
        EvenSplit, // e.g. 28 / 2 -> 2 x 14 | 2 x 14
        Other
    }

    [Serializable]
    public class DivisionTemplateStrategyTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateStrategyTag" /> from scratch.</summary>
        public DivisionTemplateStrategyTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateStrategyTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateStrategyTag" /> belongs to.</param>
        public DivisionTemplateStrategyTag(CLPPage parentPage,
                                           Origin origin,
                                           string divisionTemplateID,
                                           double dividend,
                                           double divisor,
                                           DivisionTemplateStrategies strategy,
                                           List<int> dividerValues,
                                           int divisionTemplateNumber)
            : base(parentPage, origin, divisionTemplateID, dividend, divisor, divisionTemplateNumber)
        {
            Strategy = strategy;
            DividerValues = dividerValues;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public DivisionTemplateStrategies Strategy
        {
            get { return GetValue<DivisionTemplateStrategies>(StrategyProperty); }
            set { SetValue(StrategyProperty, value); }
        }

        public static readonly PropertyData StrategyProperty = RegisterProperty("Strategy", typeof(DivisionTemplateStrategies), DivisionTemplateStrategies.Other);

        /// <summary>List of all divider values used to fill up the Division Template.</summary>
        public List<int> DividerValues
        {
            get { return GetValue<List<int>>(DividerValuesProperty); }
            set { SetValue(DividerValuesProperty, value); }
        }

        public static readonly PropertyData DividerValuesProperty = RegisterProperty("DividerValues", typeof(List<int>), () => new List<int>());

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Strategy", DivisionTemplateNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Strategy for {0} / {1}\n" + "DivisionTemplate {2} on page.\n" + "{3}: {4}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionTemplateStillOnPage ? "still" : "no longer",
                                     Strategy,
                                     string.Join(",", DividerValues.Select(x => x == 0 ? "?" : x.ToString()).Take(DividerValues.Count - 1)));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
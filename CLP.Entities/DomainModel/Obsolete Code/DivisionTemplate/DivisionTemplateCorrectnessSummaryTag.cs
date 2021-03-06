﻿using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateCorrectnessSummaryTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateCorrectnessSummaryTag" /> from scratch.</summary>
        public DivisionTemplateCorrectnessSummaryTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateCorrectnessSummaryTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateCorrectnessSummaryTag" /> belongs to.</param>
        public DivisionTemplateCorrectnessSummaryTag(CLPPage parentPage, Origin origin, Correctness correctness)
            : base(parentPage, origin)
        {
            Correctness = correctness;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness), Correctness.Unknown);

        /// <summary>Number of RepresentationCorrectnessTags set to Correct.</summary>
        public int CorrectCount
        {
            get { return GetValue<int>(CorrectCountProperty); }
            set { SetValue(CorrectCountProperty, value); }
        }

        public static readonly PropertyData CorrectCountProperty = RegisterProperty("CorrectCount", typeof(int), 0);

        /// <summary>Number of RepresentationCorrectnessTags set to PartiallyCorrect.</summary>
        public int PartiallyCorrectCount
        {
            get { return GetValue<int>(PartiallyCorrectCountProperty); }
            set { SetValue(PartiallyCorrectCountProperty, value); }
        }

        public static readonly PropertyData PartiallyCorrectCountProperty = RegisterProperty("PartiallyCorrectCount", typeof(int), 0);

        /// <summary>Number of RepresentationCorrectnessTags set to Incorrect.</summary>
        public int IncorrectCount
        {
            get { return GetValue<int>(IncorrectCountProperty); }
            set { SetValue(IncorrectCountProperty, value); }
        }

        public static readonly PropertyData IncorrectCountProperty = RegisterProperty("IncorrectCount", typeof(int), 0);

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.DivisionTemplate;

        public override string FormattedName => "Division Template Correctness Summary";

        public override string FormattedValue
        {
            get
            {
                return string.Format("Overall Correctness: {0}{1}{2}{3}",
                                     Correctness,
                                     CorrectCount == 0 ? string.Empty : string.Format("\n{0} FinalRepresentationCorrectnessTag(s) set to Correct.", CorrectCount),
                                     PartiallyCorrectCount == 0 ? string.Empty : string.Format("\n{0} FinalRepresentationCorrectnessTag(s) set to Partially Correct.", PartiallyCorrectCount),
                                     IncorrectCount == 0 ? string.Empty : string.Format("\n{0} FinalRepresentationCorrectnessTag(s) set to Incorrect.", IncorrectCount));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
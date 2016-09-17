using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RepresentationCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="RepresentationCorrectnessTag" /> from scratch.</summary>
        public RepresentationCorrectnessTag() { }

        /// <summary>Initializes <see cref="RepresentationCorrectnessTag" />.</summary>
        public RepresentationCorrectnessTag(CLPPage parentPage, Origin origin, List<string> analysisCodes)
            : base(parentPage, origin)
        {
            AnalysisCodes = analysisCodes;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>List of the representation correctness analysis codes on the page.</summary>
        public List<string> AnalysisCodes
        {
            get { return GetValue<List<string>>(AnalysisCodesProperty); }
            set { SetValue(AnalysisCodesProperty, value); }
        }

        public static readonly PropertyData AnalysisCodesProperty = RegisterProperty("AnalysisCodes", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override Category Category => Category.Representation;

        public override string FormattedName => "Representation Correctness";

        public override string FormattedValue => string.Join("\n", AnalysisCodes);

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
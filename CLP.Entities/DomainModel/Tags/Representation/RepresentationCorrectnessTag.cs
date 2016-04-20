using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
            : base(parentPage, origin) { AnalysisCodes = analysisCodes; }

        /// <summary>Initializes <see cref="RepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public RepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of the representation correctness analysis codes on the page.</summary>
        public List<string> AnalysisCodes
        {
            get { return GetValue<List<string>>(AnalysisCodesProperty); }
            set { SetValue(AnalysisCodesProperty, value); }
        }

        public static readonly PropertyData AnalysisCodesProperty = RegisterProperty("AnalysisCodes", typeof (List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Representation; }
        }

        public override string FormattedName
        {
            get { return "Representation Correctness"; }
        }

        public override string FormattedValue
        {
            get { return string.Join("\n", AnalysisCodes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
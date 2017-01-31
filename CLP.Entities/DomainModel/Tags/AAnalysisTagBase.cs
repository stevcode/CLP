using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class AAnalysisTagBase : ATagBase, IAnalysis
    {
        #region Constructors

        protected AAnalysisTagBase() { }

        protected AAnalysisTagBase(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region IAnalysis Implementation

        /// <summary>List of all the Analysis Codes the Tag generates.</summary>
        public List<string> AnalysisCodes
        {
            get { return GetValue<List<string>>(AnalysisCodesProperty); }
            set { SetValue(AnalysisCodesProperty, value); }
        }

        public static readonly PropertyData AnalysisCodesProperty = RegisterProperty("AnalysisCodes", typeof(List<string>), () => new List<string>());

        public string AnalysisCodesReport => string.Join(", ", AnalysisCodes);

        #endregion // IAnalysis Implementation
    }
}
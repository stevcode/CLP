using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
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
            get => GetValue<List<string>>(AnalysisCodesProperty);
            set => SetValue(AnalysisCodesProperty, value);
        }

        public static readonly PropertyData AnalysisCodesProperty = RegisterProperty("AnalysisCodes", typeof(List<string>), () => new List<string>());

        public string AnalysisCodesReport => string.Join(", ", AnalysisCodes);

        /// <summary>List of <see cref="ISemanticEvent" /> IDs used to generate this Tag.</summary>
        public List<string> SemanticEventIDs
        {
            get => GetValue<List<string>>(SemanticEventIDsProperty);
            set => SetValue(SemanticEventIDsProperty, value);
        }

        public static readonly PropertyData SemanticEventIDsProperty = RegisterProperty("SemanticEventIDs", typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<ISemanticEvent> SemanticEvents
        {
            get
            {
                return ParentPage.History.SemanticEvents.Where(h => SemanticEventIDs.Contains(h.ID)).OrderBy(h => h.SemanticPassNumber).ThenBy(h => h.SemanticEventIndex).ToList();
            }
        }

        #endregion // Calculated Properties

        #endregion // IAnalysis Implementation
    }
}
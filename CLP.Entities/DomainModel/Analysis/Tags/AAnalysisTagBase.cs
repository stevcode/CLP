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
        public List<string> SpreadSheetCodes
        {
            get => GetValue<List<string>>(AnalysisCodesProperty);
            set => SetValue(AnalysisCodesProperty, value);
        }

        public static readonly PropertyData AnalysisCodesProperty = RegisterProperty(nameof(SpreadSheetCodes), typeof(List<string>), () => new List<string>());

        public List<IAnalysisCode> QueryCodes
        {
            get => GetValue<List<IAnalysisCode>>(QueryCodesProperty);
            set => SetValue(QueryCodesProperty, value);
        }

        public static readonly PropertyData QueryCodesProperty = RegisterProperty(nameof(QueryCodes), typeof(List<IAnalysisCode>), () => new List<IAnalysisCode>());

        /// <summary>List of <see cref="ISemanticEvent" /> IDs used to generate this Tag.</summary>
        public List<string> SemanticEventIDs
        {
            get => GetValue<List<string>>(SemanticEventIDsProperty);
            set => SetValue(SemanticEventIDsProperty, value);
        }

        public static readonly PropertyData SemanticEventIDsProperty = RegisterProperty(nameof(SemanticEventIDs), typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<ISemanticEvent> SemanticEvents
        {
            get
            {
                return ParentPage.History.SemanticEvents.Where(e => e.SemanticPassNumber == 3)
                                 .Where(e => SemanticEventIDs.Contains(e.ID))
                                 .OrderBy(e => e.SemanticEventIndex)
                                 .ToList();
            }
        }

        #endregion // Calculated Properties

        #endregion // IAnalysis Implementation
    }
}
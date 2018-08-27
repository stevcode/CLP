using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class QueryResult
    {
        public QueryResult(QueryablePage page)
        {
            Page = page;
            AnalysisCodes = new List<AnalysisCodeContainer>();
        }

        public QueryablePage Page { get; set; }
        public List<AnalysisCodeContainer> AnalysisCodes { get; set; }
        public string ClusterName { get; set; }
        public int ClusterSize { get; set; }

        public int PageNumber => Page.PageNameComposite.PageNumber;
        public string StudentName => Page.StudentName;

        public string FormattedValue
        {
            get
            {
                var codes = string.Empty;
                foreach (var analysisCodeContainer in AnalysisCodes)
                {
                    codes += analysisCodeContainer.IsDominantSharedCode
                                 ? $"\n - **{analysisCodeContainer.Code.FormattedValue}"
                                 : $"\n - {analysisCodeContainer.Code.FormattedValue}";
                }

                return $"Page {PageNumber}, {StudentName}{codes}";
            }
        }
    }

    public class AnalysisCodeContainer
    {
        public AnalysisCodeContainer(IAnalysisCode code)
        {
            Code = code;
        }

        public IAnalysisCode Code { get; set; }
        public bool IsMatchingCode { get; set; }
        public bool IsDominantSharedCode { get; set; }
    }
}

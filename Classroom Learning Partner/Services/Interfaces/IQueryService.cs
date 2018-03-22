using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IQueryService
    {
        ClassRoster RosterToQuery { get; set; }
        Notebook NotebookToQuery { get; set; }
        List<int> PageNumbersToQuery { get; set; }
        List<string> StudentIDsToQuery { get; set; }

        void LoadQueryablePages();
        List<QueryService.QueryResult> QueryByString(string queryString);
        List<QueryService.QueryResult> QueryByConditions(List<QueryCondition> conditions);
        QueryService.Report GatherReports();
    }
}
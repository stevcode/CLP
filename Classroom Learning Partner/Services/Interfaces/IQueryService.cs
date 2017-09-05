﻿using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface IQueryService
    {
        ClassRoster RosterToQuery { get; set; }
        Notebook NotebookToQuery { get; set; }
        List<int> PageNumbersToQuery { get; set; }
        List<string> StudentIDsToQuery { get; set; }

        List<QueryService.QueryResult> RunQuery(string queryString);
    }
}
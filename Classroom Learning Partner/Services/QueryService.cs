﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Catel;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    public class QueryService : IQueryService
    {
        #region Nested Classes

        public class Query
        {
            public Query()
            {
                Constraints = new Dictionary<string, List<string>>();
                ConstraintValues = new Dictionary<string, string>();
            }

            public string QueryLabel { get; set; }
            public string Alias { get; set; }
            public Dictionary<string,List<string>> Constraints { get; set; }
            public Dictionary<string, string> ConstraintValues { get; set; }
        }

        public class QueryResult
        {
            public QueryResult()
            {
                MatchingQueryCodes = new List<IAnalysisCode>();
                AllQueryCodes = new List<IAnalysisCode>();
            }

            public string CacheFilePath { get; set; }
            public string PageID { get; set; }
            public int PageNumber { get; set; }
            public string StudentName { get; set; }
            public List<IAnalysisCode> MatchingQueryCodes { get; set; }
            public List<IAnalysisCode> AllQueryCodes { get; set; }
        }

        public class Report
        {
            public Report(string queryLabel)
            {
                QueryLabel = queryLabel;
            }

            public string QueryLabel { get; set; }
            public PrimaryReport Primary { get; set; }

        }

        public class PrimaryReport
        {
            public PrimaryReport(string constraintValueType)
            {
                Rows = new List<PrimaryRow>();

                ConstraintValueType = constraintValueType;
                MatchedEntriesLabel = "Matched\nEntries";
                MatchedInstancesLabel = "Matched\nInstances";
                TotalEntriesLabel = "Total\nEntries";
            }

            public string PrimaryQueryLabel { get;set; }

            public List<PrimaryRow> Rows { get; set; }

            public string ConstraintValueType { get; set; }
            public string MatchedEntriesLabel { get; set; }
            public string MatchedInstancesLabel { get; set; }
            public string TotalEntriesLabel { get; set; }

            public string PercentageLabel => $"%\n{MatchedInstancesLabel}\nover\n{TotalEntriesLabel}";
        }

        public class PrimaryRow
        {
            public string ConstraintValue { get; set; }
            public int MatchedEntries { get; set; }
            public int MatchedInstances { get; set; }
            public int TotalMatchedEntries { get; set; }

            public string EntriesOverTotalEntriesPercentage
            {
                get
                {
                    var percentage = Math.Round((100.0 * MatchedEntries) / TotalMatchedEntries, 2, MidpointRounding.AwayFromZero);
                    return $"{percentage:0.00}%";
                }
            }
        }

        #endregion // Nested Classes

        #region Constructor

        public QueryService()
        {
            PageNumbersToQuery = new List<int>();
            StudentIDsToQuery = new List<string>();
            Queries = new List<Query>();
            QueryCache = new Dictionary<int, List<IAnalysisCode>>();
            QueryResults = new List<QueryResult>();
        }

        #endregion // Constructor

        #region IQueryService Implementation

        public ClassRoster RosterToQuery { get; set; }

        public Notebook NotebookToQuery { get; set; }

        public List<int> PageNumbersToQuery { get; set; }

        public List<string> StudentIDsToQuery { get; set; }

        public List<Query> Queries { get; set; }

        public Dictionary<int, List<IAnalysisCode>> QueryCache { get; set; }

        public List<QueryResult> QueryResults { get; set; }

        public List<QueryResult> RunQuery(string queryString)
        {
            var queryResults = new List<QueryResult>();
            if (NotebookToQuery == null)
            {
                return queryResults;
            }

            var query = ParseQueryString(queryString);
            if (query == null)
            {
                return queryResults;
            }

            var queries = new List<Query>
                          {
                              query
                          };

            Queries = queries;

            var isQueryCached = QueryCache.Keys.Any();

            var cacheFilePath = NotebookToQuery.ContainerZipFilePath;
            var xDocuments = GetAllXDocumentsFromCache(cacheFilePath);
            foreach (var xDocument in xDocuments)
            {
                var root = xDocument.Element("CLPPage");
                if (root == null)
                {
                    continue;
                }

                #region Restrict Page Numbers

                var pageNumber = (int)root.Element("PageNumber");
                if (!PageNumbersToQuery.Contains(pageNumber))
                {
                    continue;
                }

                #endregion // Restrict Page Numbers

                #region Restrict Student IDs

                var studentID = (string)root.Element("Owner")?.Element("ID");
                if (string.IsNullOrWhiteSpace(studentID) ||
                    !StudentIDsToQuery.Contains(studentID))
                {
                    continue;
                }

                #endregion // Restrict Student IDs

                var queryCodes = GetPageQueryCodes(root);
                if (!isQueryCached)
                {
                    if (QueryCache.ContainsKey(pageNumber))
                    {
                        QueryCache[pageNumber].AddRange(queryCodes.ToList());
                    }
                    else
                    {
                        QueryCache.Add(pageNumber, queryCodes);
                    }
                }

                var isMatchingResult = IsQueryMatch(queryCodes, queries);
                if (!isMatchingResult)
                {
                    continue;
                }

                var queryResult = ParseQueryResultFromXElement(root, pageNumber, studentID, cacheFilePath);
                queryResult.MatchingQueryCodes = queryCodes.Where(c => c.AnalysisLabel == query.QueryLabel).ToList();
                queryResult.AllQueryCodes = queryCodes.ToList();
                queryResults.Add(queryResult);
            }

            QueryResults = queryResults;
            return queryResults;
        }

        public Report GatherReports()
        {
            if (!Queries.Any())
            {
                return null;
            }

            var queryLabel = Queries.First().QueryLabel;
            var allPagesPrimaryReport = new PrimaryReport("Pages")
                                        {
                                            TotalEntriesLabel = "Total\nStudents"
                                        };

            var totalStudents = 22;
            var pageNumbers = PageNumbersToQuery.ToList();
            foreach (var pageNumber in pageNumbers)
            {
                var constraintValue = pageNumber.ToString();
                var queryResultsForPage = QueryResults.Where(r => r.PageNumber == pageNumber).ToList();
                var matchedEntries = queryResultsForPage.Count;
                var matchedInstances = queryResultsForPage.Sum(r => r.MatchingQueryCodes.Count);
                var totalMatchedEntries = totalStudents;

                var primaryRow = new PrimaryRow()
                                 {
                                     ConstraintValue = constraintValue,
                                     MatchedEntries = matchedEntries,
                                     MatchedInstances = matchedInstances,
                                     TotalMatchedEntries = totalMatchedEntries
                                 };
                allPagesPrimaryReport.Rows.Add(primaryRow);
            }

            var tallyRow = new PrimaryRow()
                           {
                               ConstraintValue = "Total\nMatched\nEntries",
                               MatchedEntries = allPagesPrimaryReport.Rows.Sum(r => r.MatchedEntries),
                               MatchedInstances = allPagesPrimaryReport.Rows.Sum(r => r.MatchedInstances),
                               TotalMatchedEntries = allPagesPrimaryReport.Rows.Sum(r => r.TotalMatchedEntries)
                           };
            allPagesPrimaryReport.Rows.Add(tallyRow);

            var report = new Report(queryLabel);
            report.Primary = allPagesPrimaryReport;
            return report;
        }

        #endregion // IQueryService Implementation

        #region Methods

        private List<XDocument> GetAllXDocumentsFromCache(string cacheFilePath)
        {
            List<DataService.PageZipEntryLoader> pageZipEntryLoaders;
            
            using (var zip = ZipFile.Read(cacheFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var pageEntries = DataService.GetAllPageEntriesInCache(zip);
                pageZipEntryLoaders = DataService.GetPageZipEntryLoadersFromEntries(pageEntries).ToList();
            }

            var xDocs = pageZipEntryLoaders.Select(el => XDocument.Parse(el.XmlString)).ToList();

            return xDocs;
        }

        private QueryResult ParseQueryResultFromXElement(XElement root, int pageNumber, string studentID, string cacheFilePath)
        {
            var studentFirstName = (string)root.Element("Owner")?.Element("FirstName");
            var studentLastName = (string)root.Element("Owner")?.Element("LastName");
            var nickname = (string)root.Element("Owner")?.Element("Nickname");
            var alias = (string)root.Element("Owner")?.Element("Alias");
            var studentName = Person.CreateDisplayName(studentFirstName, studentLastName, studentID, nickname, alias);

            var queryResult = new QueryResult
                              {
                                  CacheFilePath = cacheFilePath,
                                  PageNumber = pageNumber,
                                  StudentName = studentName
                              };

            return queryResult;
        }

        private bool IsQueryMatch(List<IAnalysisCode> queryCodes, List<Query> queries)
        {
            if (!queries.Any())
            {
                return false;
            }

            var query = queries.First();
            var queryLabel = query.QueryLabel;
            return queryCodes.Any(c => c.AnalysisLabel == queryLabel);
        }

        private List<IAnalysisCode> GetPageQueryCodes(XElement root)
        {
            var queryCodes = new List<IAnalysisCode>();

            var queryCodeXElements = root.Descendants("QueryCodes").Elements().ToList();
            foreach (var queryCodeXElement in queryCodeXElements)
            {
                var analysisLabel = (string)queryCodeXElement.Element("AnalysisLabel");
                var analysisCode = new AnalysisCode(analysisLabel);

                var constraintValueXElements = queryCodeXElement.ElementAnyNS("ConstraintValues").Elements().ToList();
                foreach (var constraintValueXElement in constraintValueXElements)
                {
                    var constraintLabel = (string)constraintValueXElement.ElementAnyNS("ConstraintLabel");
                    var constraintValue = (string)constraintValueXElement.ElementAnyNS("ConstraintValue");
                    analysisCode.AddConstraint(constraintLabel, constraintValue);
                }

                queryCodes.Add(analysisCode);
            }

            return queryCodes;
        }

        private Query ParseQueryString(string queryString)
        {
            var allAliases = Codings.GetAllAnalysisAliases();
            if (!allAliases.Contains(queryString.ToUpper()))
            {
                return null;
            }

            var analysisLabel = Codings.AnalysisAliasToLabel(queryString.ToUpper());
            var query = GenerateQuery(analysisLabel);

            return query;
        }

        #endregion // Methods

        #region Static Methods

        public static Query GenerateQuery(string analysisLabel)
        {
            var query = new Query
                        {
                            QueryLabel = analysisLabel,
                            Alias = Codings.AnalysisLabelToAlias(analysisLabel),
                            Constraints = PopulateQueryWithAllConstraints(analysisLabel)
                        };

            return query;
        }

        public static Dictionary<string, List<string>> PopulateQueryWithAllConstraints(string analysisLabel)
        {
            var constraints = new Dictionary<string, List<string>>();
            var codedCorrectnessValues = Enum<Correctness>.GetValues().Select(Codings.CorrectnessToCodedCorrectness).ToList();
            var correctnessConstraintValues = new List<string>
                                              {
                                                  Codings.CONSTRAINT_VALUE_ALL
                                              };
            correctnessConstraintValues.AddRange(codedCorrectnessValues.ToList());

            switch (analysisLabel)
            {
                case Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP:
                case Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP:
                    break;
                case Codings.ANALYSIS_LABEL_CHANGED_ANSWER_AFTER_REPRESENTATION:
                    var answerChangedConstraintValues = new List<string>
                                           {
                                               Codings.CONSTRAINT_VALUE_ALL
                                           };
                    answerChangedConstraintValues.AddRange(from fromCorrectness in codedCorrectnessValues
                                                           from toCorrectness in codedCorrectnessValues
                                                           select $"{fromCorrectness}{Codings.CONSTRAINT_VALUE_ANSWER_CHANGE_DELIMITER}{toCorrectness}");

                    constraints.Add(Codings.CONSTRAINT_ANSWER_CHANGE, answerChangedConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_ANSWER_BEFORE_REPRESENTATION:
                    constraints.Add(Codings.CONSTRAINT_ANSWER_TYPE,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ALL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                    });
                    
                    constraints.Add(Codings.CONSTRAINT_ANSWER_CORRECTNESS, correctnessConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER:
                    constraints.Add(Codings.CONSTRAINT_ANSWER_TYPE,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ALL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                    });

                    constraints.Add(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, correctnessConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED:
                    constraints.Add(Codings.CONSTRAINT_REPRESENTATION_NAME,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ALL,
                                        Codings.OBJECT_ARRAY,
                                        Codings.OBJECT_NUMBER_LINE,
                                        Codings.OBJECT_STAMP,
                                        Codings.OBJECT_BINS,
                                        Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY,
                                        Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE
                                    });

                    constraints.Add(Codings.CONSTRAINT_HISTORY_STATUS,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ALL,
                                        Codings.CONSTRAINT_VALUE_HISTORY_STATUS_FINAL,
                                        Codings.CONSTRAINT_VALUE_HISTORY_STATUS_DELETED
                                    });

                    constraints.Add(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, correctnessConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_ARRAY_SKIP_COUNTING:
                case Codings.ANALYSIS_LABEL_FILL_IN_ANSWER_CORRECTNESS:
                case Codings.ANALYSIS_LABEL_PROBLEM_TYPE:
                    break;
            }


            return constraints;
        }

        #endregion // Static Methods
    }
}
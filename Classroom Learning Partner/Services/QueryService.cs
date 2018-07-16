﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Xml.Linq;
using CLP.Entities;
using CLP.MachineAnalysis;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    public class QueryService : IQueryService
    {
        #region Nested Classes

        public class QueryablePage
        {
            public QueryablePage()
            {
                MatchingAnalysisCodes = new List<IAnalysisCode>();
                AllAnalysisCodes = new List<IAnalysisCode>();
            }

            public string CacheFilePath { get; set; }
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public CLPPage.NameComposite PageNameComposite { get; set; }
            public List<IAnalysisCode> MatchingAnalysisCodes { get; set; }
            public List<IAnalysisCode> AllAnalysisCodes { get; set; }

            public string FormattedValue
            {
                get { return $"Page {PageNameComposite.PageNumber}, {StudentName}\n - {string.Join("\n - ", MatchingAnalysisCodes.Select(q => q.FormattedValue))}"; }
            }

            #region Methods

            public double Distance(QueryablePage otherPage)
            {
                return 0.0;
            }

            #endregion // Methods
        }

        public class QueryResult
        {
            public QueryResult(QueryablePage page)
            {
                Page = page;
                MatchingQueryCodes = new List<IAnalysisCode>();
            }

            public QueryablePage Page { get; set; }
            public List<IAnalysisCode> MatchingQueryCodes { get; set; }
            public string Cluster { get; set; }

            public int PageNumber => Page.PageNameComposite.PageNumber;
            public string StudentName => Page.StudentName;

            public string FormattedValue
            {
                get { return $"Page {PageNumber}, {StudentName}\n - {string.Join("\n - ", MatchingQueryCodes.Select(q => q.FormattedValue))}"; }
            }
        }

        #endregion // Nested Classes

        #region Constructor

        public QueryService()
        {
            PageNumbersToQuery = new List<int>();
            StudentIDsToQuery = new List<string>();
            QueryablePages = new List<QueryablePage>();
        }

        #endregion // Constructor

        #region IQueryService Implementation

        public ClassRoster RosterToQuery { get; set; }

        public Notebook NotebookToQuery { get; set; }

        public List<int> PageNumbersToQuery { get; set; }

        public List<string> StudentIDsToQuery { get; set; }

        public Queries SavedQueries { get; set; }

        public List<QueryablePage> QueryablePages { get; set; }

        public void LoadQueryablePages()
        {
            QueryablePages.Clear();

            var cacheFilePath = NotebookToQuery.ContainerZipFilePath;
            var pageXDocuments = GetAllPageXDocumentsFromCache(cacheFilePath);
            foreach (var pageXDocument in pageXDocuments)
            {
                var root = pageXDocument.Element("CLPPage");
                if (root == null)
                {
                    continue;
                }

                #region Student Information

                var isStudent = (bool)root.Element("Owner")?.Element("IsStudent");
                if (!isStudent)
                {
                    continue;
                }

                var studentID = (string)root.Element("Owner")?.Element("ID");
                var studentFirstName = (string)root.Element("Owner")?.Element("FirstName");
                var studentLastName = (string)root.Element("Owner")?.Element("LastName");
                var nickname = (string)root.Element("Owner")?.Element("Nickname");
                var alias = (string)root.Element("Owner")?.Element("Alias");
                var studentName = Person.CreateDisplayName(studentFirstName, studentLastName, studentID, nickname, alias);

                #endregion // Student Information

                #region PageNameComposite

                var pageID = (string)root.Element("ID");
                var pageNumber = (int)root.Element("PageNumber");
                var subPageNumber = (int)root.Element("SubPageNumber");
                var differentiationLevel = (string)root.Element("DifferentiationLevel");
                var versionIndex = (uint)root.Element("VersionIndex");
                var pageNameComposite = new CLPPage.NameComposite
                                        {
                                            ID = pageID,
                                            PageNumber = pageNumber,
                                            SubPageNumber = subPageNumber,
                                            DifferentiationLevel = differentiationLevel,
                                            VersionIndex = versionIndex
                                        };

                #endregion // PageNameComposite

                var queryCodes = GetPageAnalysisCodes(root);

                var queryablePage = new QueryablePage
                                    {
                                        CacheFilePath = cacheFilePath,
                                        StudentID = studentID,
                                        StudentName = studentName,
                                        PageNameComposite = pageNameComposite,
                                        AllAnalysisCodes = queryCodes.ToList()
                                    };

                QueryablePages.Add(queryablePage);
            }
        }

        public void LoadSavedQueries()
        {
            var cacheFilePath = NotebookToQuery.ContainerZipFilePath;
            SavedQueries = DataService.LoadQueriesFromCLPContainer(cacheFilePath) ?? new Queries();
            SavedQueries.ContainerZipFilePath = NotebookToQuery.ContainerZipFilePath;
        }

        public List<QueryResult> RunQuery(AnalysisCodeQuery query)
        {
            if (NotebookToQuery == null || 
                query == null)
            {
                return new List<QueryResult>();
            }

            var queryResults = new List<QueryResult>();

            foreach (var queryablePage in QueryablePages)
            {
                var isMatchingResult = PageNumbersToQuery.Contains(queryablePage.PageNameComposite.PageNumber) && IsPageAMatch(queryablePage, query);
                if (!isMatchingResult)
                {
                    continue;
                }

                var queryResult = new QueryResult(queryablePage);
                queryResult.MatchingQueryCodes = queryablePage.AllAnalysisCodes.ToList();
                queryResults.Add(queryResult);
            }

            return queryResults;
        }

        public List<QueryResult> Cluster()
        {
            if (NotebookToQuery == null)
            {
                return new List<QueryResult>();
            }

            const int MAX_EPSILON = 1000;
            const int MINIMUM_PAGES_IN_CLUSTER = 1;

            double DistanceEquation(QueryablePage p1, QueryablePage p2) => Math.Sqrt(p1.Distance(p2));
            var optics = new OPTICS<QueryablePage>(MAX_EPSILON, MINIMUM_PAGES_IN_CLUSTER, QueryablePages, DistanceEquation);
            optics.BuildReachability();
            var reachabilityDistances = optics.ReachabilityDistances().ToList();

            const double CLUSTERING_EPSILON = 51.0;

            var currentCluster = new List<QueryablePage>();
            var allClusteredQueryablePages = new List<QueryablePage>();
            var firstQueryablePageIndex = (int)reachabilityDistances[0].OriginalIndex;
            var firstQueryablePage = QueryablePages[firstQueryablePageIndex];
            currentCluster.Add(firstQueryablePage);
            allClusteredQueryablePages.Add(firstQueryablePage);

            var clusters = new List<List<QueryablePage>>();

            for (var i = 1; i < reachabilityDistances.Count; i++)
            {
                var queryablePageIndex = (int)reachabilityDistances[i].OriginalIndex;
                var queryablePage = QueryablePages[queryablePageIndex];

                // Epsilon cluster decision.
                var currentReachabilityDistance = reachabilityDistances[i].ReachabilityDistance;
                if (currentReachabilityDistance < CLUSTERING_EPSILON)
                {
                    currentCluster.Add(queryablePage);
                    allClusteredQueryablePages.Add(queryablePage);
                    continue;
                }

                var fullCluster = currentCluster.ToList();
                currentCluster.Clear();
                currentCluster.Add(queryablePage);
                allClusteredQueryablePages.Add(queryablePage);
                clusters.Add(fullCluster);
            }

            if (currentCluster.Any())
            {
                var finalCluster = currentCluster.ToList();
                clusters.Add(finalCluster);
            }

            var anomaliesCluster = QueryablePages.Where(qp => !allClusteredQueryablePages.Contains(qp)).ToList();

            var queryResults = new List<QueryResult>();
            foreach (var queryablePage in anomaliesCluster)
            {
                var queryResult = new QueryResult(queryablePage)
                                  {
                                      MatchingQueryCodes = queryablePage.AllAnalysisCodes.ToList(),
                                      Cluster = "Anomalies"
                                  };
                queryResults.Add(queryResult);
            }

            var clusterCount = 1;
            foreach (var cluster in clusters)
            {
                foreach (var queryablePage in cluster)
                {
                    var queryResult = new QueryResult(queryablePage)
                                      {
                                          MatchingQueryCodes = queryablePage.AllAnalysisCodes.ToList(),
                                          Cluster = $"Cluster {clusterCount}"
                                      };
                    queryResults.Add(queryResult);
                }

                clusterCount++;
            }
            
            return queryResults;
        }

        #endregion // IQueryService Implementation

        #region Methods

        private List<XDocument> GetAllPageXDocumentsFromCache(string cacheFilePath)
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

        private bool IsPageAMatch(QueryablePage queryablePage, AnalysisCodeQuery query)
        {
            if (query == null ||
                queryablePage == null)
            {
                return false;
            }

            var isFirstConditionAMatch = IsPageAMatchForQueryPart(queryablePage, query.FirstCondition);
            var isSecondConditionAMatch = IsPageAMatchForQueryPart(queryablePage, query.SecondCondition);

            var isAMatch = false;
            switch (query.Conditional)
            {
                case QueryConditionals.None:
                    isAMatch = isFirstConditionAMatch;
                    break;
                case QueryConditionals.And:
                    isAMatch = isFirstConditionAMatch && isSecondConditionAMatch;
                    break;
                case QueryConditionals.Or:
                    isAMatch = isFirstConditionAMatch || isSecondConditionAMatch;
                    break;
            }

            return isAMatch;
        }

        private bool IsPageAMatchForQueryPart(QueryablePage queryablePage, IQueryPart queryPart)
        {
            if (queryPart == null)
            {
                return false;
            }

            if (queryPart is AnalysisCodeQuery query)
            {
                return IsPageAMatch(queryablePage, query);
            }

            if (!(queryPart is AnalysisCode queryCode))
            {
                return false;
            }

            var matchingCodes = new List<IAnalysisCode>();
            foreach (var analysisCode in queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == queryCode.AnalysisCodeLabel))
            {
                var isAMatch = true;
                foreach (var queryConstraint in queryCode.Constraints.Where(c => c.IsQueryable && c.ConstraintValue != Codings.CONSTRAINT_VALUE_ANY))
                {
                    var analysisConstraint = analysisCode.Constraints.FirstOrDefault(c => c.ConstraintLabel == queryConstraint.ConstraintLabel);
                    if (analysisConstraint == null)
                    {
                        isAMatch = false;
                        break;
                    }

                    if (queryConstraint.ConstraintValue == analysisConstraint.ConstraintValue)
                    {
                        continue;
                    }

                    if (queryConstraint.ConstraintValue == Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_NONE &&
                        (analysisConstraint.ConstraintValue == Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_INK_ONLY || 
                         analysisConstraint.ConstraintValue == Codings.CONSTRAINT_VALUE_REPRESENTATION_NAME_BLANK_PAGE))
                    {
                        continue;
                    }

                    isAMatch = false;
                    break;
                }

                if (isAMatch)
                {
                    matchingCodes.Add(analysisCode);
                }
            }

            return matchingCodes.Any();
        }

        private List<IAnalysisCode> GetPageAnalysisCodes(XElement root)
        {
            var analysisCodes = new List<IAnalysisCode>();

            var queryCodeXElements = root.Descendants("QueryCodes").Elements().ToList();        // TODO: refactor tag contents to only us AnalysisCodes
            foreach (var queryCodeXElement in queryCodeXElements)
            {
                var analysisLabel = (string)queryCodeXElement.Element("AnalysisCodeLabel");
                var analysisCode = new AnalysisCode(analysisLabel);

                var constraintValueXElements = queryCodeXElement.ElementAnyNS("Constraints").Elements().ToList();
                foreach (var constraintValueXElement in constraintValueXElements)
                {
                    var constraintLabel = (string)constraintValueXElement.ElementAnyNS("ConstraintLabel");
                    var constraintValue = (string)constraintValueXElement.ElementAnyNS("ConstraintValue");
                    analysisCode.AddConstraint(constraintLabel, constraintValue);
                }

                analysisCodes.Add(analysisCode);
            }

            return analysisCodes;
        }

        #endregion // Methods

    }
}

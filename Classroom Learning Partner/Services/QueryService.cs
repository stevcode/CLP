using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Linq;
using Classroom_Learning_Partner.Views;
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
                var distance = 0.0;

                // Dist v 1, Label Count
                //var analysisCodeTypes = AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
                //var analysisCodeTypesOther = otherPage.AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
                //distance += Math.Abs(analysisCodeTypes.Count - analysisCodeTypesOther.Count);

                // Dist v 2, ???
                distance += Blah(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_NAME_LAX);
                distance += Blah(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_HISTORY_STATUS);
                distance += Blah(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);

                // Dist v 3, Pure Hamming between all code constraint values on both pages
                //distance = ALL_PURE(this, otherPage);

                return distance;
            }

            private static double ALL_PURE(QueryablePage page1, QueryablePage page2)
            {
                var distance = 0.0;
                foreach (var code1 in page1.AllAnalysisCodes)
                {
                    foreach (var code2 in page2.AllAnalysisCodes)
                    {
                        distance += HammingDistanceBetweenAnalysisCodes(code1, code2);
                    }
                }

                return distance;
            }

            private static double HammingDistanceBetweenAnalysisCodes(IAnalysisCode code1, IAnalysisCode code2)
            {
                if (code1.AnalysisCodeLabel != code2.AnalysisCodeLabel)
                {
                    return Math.Max(code1.Constraints.Count, code2.Constraints.Count);
                }

                if (code1.Constraints.Count != code2.Constraints.Count)
                {
                    // Print error?
                    return Math.Max(code1.Constraints.Count, code2.Constraints.Count);
                }

                var distance = 0;
                for (var i = 0; i < code1.Constraints.Count; i++)
                {
                    var constraint1 = code1.Constraints[i];
                    var constraint2 = code2.Constraints[i];

                    if (constraint1.ConstraintValue != constraint2.ConstraintValue)
                    {
                        distance += 1;
                    }
                }

                return distance;
            }

            private static double Blah(QueryablePage page1, QueryablePage page2, string analysisCodeLabel, string constraintLabel)
            {
                var codes = page1.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();
                var codesOther = page2.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();

                var groups = codes.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);
                var groupsOther = codesOther.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);

                var groupings = groups.ToDictionary(@group => @group.Key, @group => @group.Count());
                var groupingsOther = groupsOther.ToDictionary(@group => @group.Key, @group => @group.Count());

                var distance = 0.0;
                foreach (var groupingsKey in groupings.Keys)
                {
                    if (groupingsOther.ContainsKey(groupingsKey))
                    {
                        distance += Math.Abs(groupings[groupingsKey] - groupingsOther[groupingsKey]);
                    }
                    else
                    {
                        distance += groupings[groupingsKey];
                    }
                }

                return distance;
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

            //var queryablePages = new List<QueryablePage>();
            //foreach (var queryablePage in QueryablePages.Where(qp => qp.PageNameComposite.PageNumber == 2))
            //{
            //    queryablePages.Add(queryablePage);
            //}
            var queryablePages = QueryablePages.ToList();

            const int MAX_EPSILON = 1000;
            const int MINIMUM_PAGES_IN_CLUSTER = 1;

            double DistanceEquation(QueryablePage p1, QueryablePage p2) => Math.Sqrt(p1.Distance(p2));
            var optics = new OPTICS<QueryablePage>(MAX_EPSILON, MINIMUM_PAGES_IN_CLUSTER, queryablePages, DistanceEquation);
            optics.BuildReachability();
            var reachabilityDistances = optics.ReachabilityDistances().ToList();

            var normalizedReachabilityPlot = reachabilityDistances.Select(i => new Point(0, i.ReachabilityDistance)).Skip(1).ToList();
            var plotView = new OPTICSReachabilityPlotView()
                           {
                               Owner = Application.Current.MainWindow,
                               WindowStartupLocation = WindowStartupLocation.Manual,
                               Reachability = normalizedReachabilityPlot
                           };
            plotView.Show();

            const double CLUSTERING_EPSILON = 0.33;

            var currentCluster = new List<QueryablePage>();
            var allClusteredQueryablePages = new List<QueryablePage>();
            var firstQueryablePageIndex = (int)reachabilityDistances[0].OriginalIndex;
            var firstQueryablePage = queryablePages[firstQueryablePageIndex];
            currentCluster.Add(firstQueryablePage);
            allClusteredQueryablePages.Add(firstQueryablePage);

            var clusters = new List<List<QueryablePage>>();

            for (var i = 1; i < reachabilityDistances.Count; i++)
            {
                var queryablePageIndex = (int)reachabilityDistances[i].OriginalIndex;
                var queryablePage = queryablePages[queryablePageIndex];

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

            var anomaliesCluster = queryablePages.Where(qp => !allClusteredQueryablePages.Contains(qp)).ToList();

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

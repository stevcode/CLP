using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using CLP.MachineAnalysis;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    public class QueryService : IQueryService
    {
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

                var queryResult = new QueryResult(queryablePage)
                                  {
                                      AnalysisCodes = queryablePage.AllAnalysisCodes.Select(c => new AnalysisCodeContainer(c)).ToList()
                                  };
                queryResults.Add(queryResult);
            }

            return queryResults;
        }

        public List<QueryResult> Cluster(List<QueryablePage> queryablePages)
        {
            if (NotebookToQuery == null)
            {
                return new List<QueryResult>();
            }

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

            #region Calculate Ranges

            var xMax = 0.0;
            var yMax = 0.0;
            var zMax = 0.0;
            var xMin = double.MaxValue;
            var yMin = double.MaxValue;
            var zMin = double.MaxValue;
            foreach (var queryablePage in QueryablePages)
            {
                xMax = Math.Max(xMax, queryablePage.StudentActionDistance);
                yMax = Math.Max(yMax, queryablePage.AnalysisDistance);
                zMax = Math.Max(zMax, queryablePage.ProblemStructureDistance);

                xMin = Math.Min(xMin, queryablePage.StudentActionDistance);
                yMin = Math.Min(yMin, queryablePage.AnalysisDistance);
                zMin = Math.Min(zMin, queryablePage.ProblemStructureDistance);
            }

            CLogger.AppendToLog($"***Current Range***\n" + $"Student Action: {xMin} - {xMax}\n" + $"AnalysisDistance: {yMin} - {yMax}\n" + $"Problem Structure: {zMin} - {zMax}");

            #endregion // Calculate Ranges

            #region Scatter Plot Pages

            var graphViewModel = new GraphViewModel(QueryablePages);
            var graphView = new GraphView(graphViewModel);
            graphView.ShowDialog();

            #endregion // Scatter Plot Pages

            var clusteringEpsilon = QueryablePage.GetClusteringEpsilon();

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
                if (currentReachabilityDistance < clusteringEpsilon)
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
                                      AnalysisCodes = queryablePage.AllAnalysisCodes.Select(c => new AnalysisCodeContainer(c)).ToList(),
                                      ClusterName = "Anomalies"
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
                                          AnalysisCodes = queryablePage.AllAnalysisCodes.Select(c => new AnalysisCodeContainer(c)).ToList(),
                                          ClusterName = cluster.Count == 1 ? "OUTLIERS" : $"Cluster {clusterCount:D3}",
                                          ClusterSize = cluster.Count
                                      };

                    queryResults.Add(queryResult);
                }

                if (cluster.Count != 1)
                {
                    clusterCount++;
                }
            }

            FindDominantSharedCodes(queryResults);
            
            return queryResults;
        }

        #endregion // IQueryService Implementation

        #region Methods

        private void FindDominantSharedCodes(List<QueryResult> results)
        {
            foreach (var group in results.GroupBy(r => r.ClusterName).Where(g => g.Key != "OUTLIERS"))
            {
                var sizes = new Dictionary<int, List<AnalysisCodeContainer>>();

                var queryResults = group.ToList();
                for (var i = 0; i < queryResults.Count - 1; i++)
                {
                    var result1 = queryResults[i];
                    for (var j = i + 1; j < queryResults.Count; j++)
                    {
                        var result2 = queryResults[j];

                        foreach (var analysisCodeContainer1 in result1.AnalysisCodes)
                        {
                            foreach (var analysisCodeContainer2 in result2.AnalysisCodes)
                            {
                                var size = CompareAnalysisCodes(analysisCodeContainer1.Code, analysisCodeContainer2.Code);
                                if (!sizes.ContainsKey(size))
                                {
                                    sizes.Add(size, new List<AnalysisCodeContainer>());
                                }

                                if (!sizes[size].Contains(analysisCodeContainer1))
                                {
                                    sizes[size].Add(analysisCodeContainer1);
                                }

                                if (!sizes[size].Contains(analysisCodeContainer2))
                                {
                                    sizes[size].Add(analysisCodeContainer2);
                                }
                            }
                        }

                    }
                }

                if (sizes.Keys.Count == 0)
                {
                    continue;
                }

                var largestSize = sizes.Keys.Max();
                if (largestSize <= 0)
                {
                    continue;
                }

                foreach (var analysisCodeContainer in sizes[largestSize])
                {
                    analysisCodeContainer.IsDominantSharedCode = true;
                }
            }
        }

        private int CompareAnalysisCodes(IAnalysisCode code1, IAnalysisCode code2)
        {
            var matchingConstraintCount = 0;
            foreach (var constraint1 in code1.Constraints)
            {
                foreach (var constraint2 in code1.Constraints)
                {
                    if (constraint1.IsQueryable &&
                        constraint2.IsQueryable &&
                        constraint1.ConstraintLabel == constraint2.ConstraintLabel &&
                        constraint1.ConstraintValue == constraint2.ConstraintValue)
                    {
                        matchingConstraintCount++;
                    }
                }
            }

            if (code1.AnalysisCodeLabel == code2.AnalysisCodeLabel)
            {
                matchingConstraintCount *= 2;
            }

            return matchingConstraintCount;
        }

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

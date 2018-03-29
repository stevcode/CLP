using System;
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

            if (!(queryPart is AnalysisCode analysisCode))
            {
                return false;
            }

            var matchingCodes = queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCode.AnalysisCodeLabel).ToList();
            if (!analysisCode.Constraints.Any(c => c.IsQueryable && c.ConstraintValue != Codings.CONSTRAINT_VALUE_ANY))
            {
                return matchingCodes.Any();
            }

            foreach (var constraint in analysisCode.Constraints.Where(c => c.IsQueryable))
            {
                var constraints = queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCode.AnalysisCodeLabel).SelectMany(c => c.Constraints).ToList();
                var matchingConstraintValues = constraints.Where(c => c.ConstraintLabel == constraint.ConstraintLabel).ToList();
                if (!matchingConstraintValues.Any())
                {
                    return false;
                }

                var queryConstraintValue = constraint.ConstraintValue;
                if (queryConstraintValue == Codings.CONSTRAINT_VALUE_ANY ||
                    matchingConstraintValues.Any(c => c.ConstraintValue == queryConstraintValue))
                {
                    continue;
                }

                return false;
            }

            return true;
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

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

        public class Query
        {
            public Query()
            {
                Constraints = new Dictionary<string, List<string>>();
                ConstraintValues = new Dictionary<string, string>();
            }

            public string QueryLabel { get; set; }
            public Dictionary<string, List<string>> Constraints { get; set; }
            public Dictionary<string, string> ConstraintValues { get; set; }
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
            QueryResults = new List<QueryResult>();
        }

        #endregion // Constructor

        #region IQueryService Implementation

        public ClassRoster RosterToQuery { get; set; }

        public Notebook NotebookToQuery { get; set; }

        public List<int> PageNumbersToQuery { get; set; }

        public List<string> StudentIDsToQuery { get; set; }

        public Queries SavedQueries { get; set; }

        public List<QueryablePage> QueryablePages { get; set; }

        public List<QueryResult> QueryResults { get; set; }

        public Query LastQuery { get; set; }

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

                var queryCodes = GetPageQueryCodes(root);

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

        public List<QueryResult> QueryByString(string queryString)
        {
            var queryResults = new List<QueryResult>();
            if (NotebookToQuery == null)
            {
                return queryResults;
            }

            var query = ParseQueryString(queryString);
            LastQuery = query;
            if (query == null)
            {
                return queryResults;
            }

            foreach (var queryablePage in QueryablePages)
            {

                var isMatchingResult = IsPageAMatch(queryablePage, query) && PageNumbersToQuery.Contains(queryablePage.PageNameComposite.PageNumber);
                if (!isMatchingResult)
                {
                    continue;
                }

                var queryResult = new QueryResult(queryablePage);
                queryResult.MatchingQueryCodes = queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == query.QueryLabel).ToList();
                queryResults.Add(queryResult);
            }

            QueryResults = queryResults;
            return queryResults;
        }

        public List<QueryResult> QueryByConditions(List<AnalysisCode> conditions)
        {
            if (NotebookToQuery == null ||
                !conditions.Any())
            {
                return new List<QueryResult>();
            }

            var queries = conditions.Select(ParseCondition).ToList();
            var queryResults = new List<QueryResult>();

            foreach (var queryablePage in QueryablePages)
            {
                var isMatchingResult = queries.All(q => IsPageAMatch(queryablePage, q)) && PageNumbersToQuery.Contains(queryablePage.PageNameComposite.PageNumber);
                if (!isMatchingResult)
                {
                    continue;
                }

                var queryResult = new QueryResult(queryablePage);
                queryResult.MatchingQueryCodes = queryablePage.AllAnalysisCodes.Where(c => queries.Any(q => q.QueryLabel == c.AnalysisCodeLabel)).ToList();
                queryResults.Add(queryResult);
            }

            QueryResults = queryResults;
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

        private bool IsPageAMatch(QueryablePage queryablePage, Query query)
        {
            if (query == null ||
                queryablePage == null)
            {
                return false;
            }

            var matchingCodes = queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == query.QueryLabel).ToList();
            if (!query.ConstraintValues.Keys.Any())
            {
                return matchingCodes.Any();
            }

            foreach (var constraint in query.ConstraintValues.Keys)
            {
                var constraintValues = queryablePage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == query.QueryLabel).SelectMany(c => c.Constraints).ToList();
                var matchingConstraintValues = constraintValues.Where(c => c.ConstraintLabel == constraint).ToList();
                if (!matchingConstraintValues.Any())
                {
                    return false;
                }

                var queryConstraintValue = query.ConstraintValues[constraint];
                if (queryConstraintValue == Codings.CONSTRAINT_VALUE_ANY ||
                    matchingConstraintValues.Any(c => c.ConstraintValue.Contains(queryConstraintValue)))
                {
                    continue;
                }

                return false;
            }

            return true;
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
                return Specials(queryString);
            }

            var analysisLabel = Codings.AnalysisAliasToLabel(queryString.ToUpper());
            var query = GenerateQuery(analysisLabel);

            return query;
        }

        private Query Specials(string queryString)
        {
            var specials = new List<string>
                           {
                               "NL",
                               "ARR",
                               "STAMP",
                               "BINS"
                           };
            if (!specials.Contains(queryString.ToUpper()))
            {
                return null;
            }

            var query = GenerateQuery(Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED);
            query.ConstraintValues.Add(Codings.CONSTRAINT_REPRESENTATION_NAME, queryString.ToUpper());
            return query;
        }

        private Query ParseCondition(AnalysisCode analysisCode)
        {
            if (analysisCode == null)
            {
                return null;
            }

            var analysisLabel = analysisCode.AnalysisCodeLabel;
            var query = GenerateQuery(analysisLabel);
            foreach (var conditionConstraint in analysisCode.Constraints.Where(c => c.IsQueryable))
            {
                query.ConstraintValues.Add(conditionConstraint.ConstraintLabel, conditionConstraint.ConstraintValue);
            }

            return query;
        }

        #endregion // Methods

        #region Static Methods

        public static Query GenerateQuery(string analysisLabel)
        {
            var query = new Query
                        {
                            QueryLabel = analysisLabel,
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
                                                  Codings.CONSTRAINT_VALUE_ANY
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
                                               Codings.CONSTRAINT_VALUE_ANY
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
                                        Codings.CONSTRAINT_VALUE_ANY,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                    });
                    
                    constraints.Add(Codings.CONSTRAINT_ANSWER_CORRECTNESS, correctnessConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATION_AFTER_ANSWER:
                    constraints.Add(Codings.CONSTRAINT_ANSWER_TYPE,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ANY,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_FINAL,
                                        Codings.CONSTRAINT_VALUE_ANSWER_TYPE_INTERMEDIARY
                                    });

                    constraints.Add(Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS, correctnessConstraintValues);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED:
                    constraints.Add(Codings.CONSTRAINT_REPRESENTATION_NAME,
                                    new List<string>
                                    {
                                        Codings.CONSTRAINT_VALUE_ANY,
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
                                        Codings.CONSTRAINT_VALUE_ANY,
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

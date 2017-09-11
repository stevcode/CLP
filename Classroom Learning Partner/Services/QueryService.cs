using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
            public string QueryLabel { get; set; }
            public string Alias { get; set; }
            public Dictionary<string,List<string>> Constraints { get; set; }
            public Dictionary<string, string> ConstraintValues { get; set; }
        }

        public class QueryResult
        {
            public string CacheFilePath { get; set; }
            public string PageID { get; set; }
            public int PageNumber { get; set; }
            public string StudentName { get; set; }
        }

        #endregion // Nested Classes

        #region Constructor

        public QueryService()
        {
            PageNumbersToQuery = new List<int>();
            StudentIDsToQuery = new List<string>();
            Queries = new List<Query>();
        }

        #endregion // Constructor

        #region IQueryService Implementation

        public ClassRoster RosterToQuery { get; set; }

        public Notebook NotebookToQuery { get; set; }

        public List<int> PageNumbersToQuery { get; set; }

        public List<string> StudentIDsToQuery { get; set; }

        public List<Query> Queries { get; set; }

        public List<QueryResult> RunQuery(string queryString)
        {
            var queryResults = new List<QueryResult>();
            if (NotebookToQuery == null)
            {
                return queryResults;
            }

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

                var analysisCodes = root.Descendants("AnalysisCodes").Descendants().Select(e => e.Value).ToList();
                var isMatchingResult = analysisCodes.Any(c => c.Contains(queryString.ToUpper()));
                if (!isMatchingResult)
                {
                    continue;
                }

                var queryResult = ParseQueryResultFromXElement(root, pageNumber, studentID, cacheFilePath);
                queryResults.Add(queryResult);
            }

            return queryResults;
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

            foreach (var query in queries)
            {
          //      CLogger.AppendToLog(queryCodes.FirstOrDefault());
            }

            return false;
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

        #endregion // Methods
    }
}

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
        }

        #endregion // Constructor

        #region IQueryService Implementation

        public ClassRoster RosterToQuery { get; set; }

        public Notebook NotebookToQuery { get; set; }

        public List<int> PageNumbersToQuery { get; set; }

        public List<string> StudentIDsToQuery { get; set; }

        public List<QueryResult> RunQuery(string queryString)
        {
            if (NotebookToQuery == null)
            {
                return null;
            }

            // TODO: Sanatize query string from master list of all available analysis codes. convert ABR to FABR/IABR

            var queryResults = new List<QueryResult>();

            List<DataService.PageZipEntryLoader> pageZipEntryLoaders;
            var cacheFilePath = NotebookToQuery.ContainerZipFilePath;
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

            foreach (var xDocument in xDocs)
            {
                var pageNumber = xDocument.Descendants("PageNumber").First().Value.ToInt();
                if (pageNumber == null ||
                    !PageNumbersToQuery.Contains(pageNumber.Value))
                {
                    continue;
                }

                var studentID = xDocument.Descendants("Owner").First().Descendants("ID").First().Value;
                if (string.IsNullOrWhiteSpace(studentID) ||
                    !StudentIDsToQuery.Contains(studentID))
                {
                    continue;
                }

                var analysisCodes = xDocument.Descendants("AnalysisCodes").Descendants().Select(e => e.Value).ToList();
                var isMatchingResult = analysisCodes.Any(c => c.StartsWith(queryString.ToUpper()));
                if (!isMatchingResult)
                {
                    continue;
                }

                var studentFirstName = xDocument.Descendants("Owner").First().Descendants("FirstName").First().Value;
                var studentLastName = xDocument.Descendants("Owner").First().Descendants("LastName").First().Value;

                var studentName = $"{studentFirstName} {studentLastName}";

                var queryResult = new QueryResult
                                  {
                                      CacheFilePath = cacheFilePath,
                                      PageNumber = pageNumber.Value,
                                      StudentName = studentName
                                  };
                queryResults.Add(queryResult);
            }

            return queryResults;
        }

        #endregion // IQueryService Implementation
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.ViewModels
{
    public class QueryPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;

        public QueryPanelViewModel(IDataService dataService)
        {
            Argument.IsNotNull(() => dataService);

            _dataService = dataService;

            InitializedAsync += QueryPanelViewModel_InitializedAsync;

            InitializeCommands();
        }

        #region Events

        private async Task QueryPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
        }

        #endregion // Events

        #region Bindings

        /// <summary>String to run the query on.</summary>
        public string QueryString
        {
            get => GetValue<string>(QueryStringProperty);
            set => SetValue(QueryStringProperty, value);
        }

        public static readonly PropertyData QueryStringProperty = RegisterProperty(nameof(QueryString), typeof(string), string.Empty);

        /// <summary>Temp results of query.</summary>
        public ObservableCollection<string> QueryResults
        {
            get => GetValue<ObservableCollection<string>>(QueryResultsProperty);
            set => SetValue(QueryResultsProperty, value);
        }

        public static readonly PropertyData QueryResultsProperty =
            RegisterProperty(nameof(QueryResults), typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            RunQueryCommand = new Command(OnRunQueryCommandExecute);
        }

        /// <summary>Runs a query using the current QueryString.</summary>
        public Command RunQueryCommand { get; private set; }

        private void OnRunQueryCommandExecute()
        {
            QueryResults.Clear();
            Query();
        }

        #endregion // Commands

        private void Query()
        {
            var queryString = QueryString;
            if (string.IsNullOrWhiteSpace(queryString))
            {
                MessageBox.Show("Can't query empty string.");
                return;
            }

            if (queryString.ToUpper() != "ABR")
            {
                MessageBox.Show("Query not recognized.");
                return;
            }

            var currentNotebook = _dataService.CurrentNotebook;
            var currentZip = currentNotebook.ContainerZipFilePath;
            var pageZipEntryLoaders = new List<DataService.PageZipEntryLoader>();
            
            using (var zip = ZipFile.Read(currentZip))
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
                var analysisCodes = xDocument.Descendants("AnalysisCodes").Descendants().Select(e => e.Value).ToList();
                var isABR = analysisCodes.Any(c => c.Contains("ABR"));
                if (isABR)
                {
                    var pageNumber = xDocument.Descendants("PageNumber").First().Value;
                    var studentFirstName = xDocument.Descendants("Owner").First().Descendants("FirstName").First().Value;
                    var studentLastName = xDocument.Descendants("Owner").First().Descendants("LastName").First().Value;

                    var studentName = $"{studentFirstName} {studentLastName}";

                    var queryResult = $"Page {pageNumber}, {studentName}";
                    QueryResults.Add(queryResult);
                }
            }
        }
    }
}
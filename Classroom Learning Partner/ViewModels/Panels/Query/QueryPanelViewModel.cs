using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.ViewModels
{
    public class QueryPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IQueryService _queryService;

        public QueryPanelViewModel(IDataService dataService, IQueryService queryService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => queryService);

            _dataService = dataService;
            _queryService = queryService;

            InitializeQueryConstraints();

            InitializedAsync += QueryPanelViewModel_InitializedAsync;

            InitializeCommands();

            CurrentCodeQuery = new AnalysisCodeQuery();
        }

        #region Events

        private async Task QueryPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
        }

        #endregion // Events

        #region Bindings

        public override double InitialLength => 500.0;

        public AnalysisCodeQuery CurrentCodeQuery
        {
            get => GetValue<AnalysisCodeQuery>(CurrentCodeQueryProperty);
            set => SetValue(CurrentCodeQueryProperty, value);
        }

        public static readonly PropertyData CurrentCodeQueryProperty = RegisterProperty(nameof(CurrentCodeQuery), typeof(AnalysisCodeQuery), null);

        public Queries SavedQueries
        {
            get => GetValue<Queries>(SavedQueriesProperty);
            set => SetValue(SavedQueriesProperty, value);
        }

        public static readonly PropertyData SavedQueriesProperty = RegisterProperty(nameof(SavedQueries), typeof(Queries), null);

        /// <summary>Temp results of query.</summary>
        public ObservableCollection<QueryService.QueryResult> QueryResults
        {
            get => GetValue<ObservableCollection<QueryService.QueryResult>>(QueryResultsProperty);
            set => SetValue(QueryResultsProperty, value);
        }

        public static readonly PropertyData QueryResultsProperty =
            RegisterProperty(nameof(QueryResults), typeof(ObservableCollection<QueryService.QueryResult>), () => new ObservableCollection<QueryService.QueryResult>());

        #endregion // Bindings

        #region Methods

        private void InitializeQueryConstraints()
        {
            if (_queryService.NotebookToQuery != null &&
                _queryService.NotebookToQuery == _dataService.CurrentNotebook)
            {
                return;
            }

            _queryService.RosterToQuery = _dataService.CurrentClassRoster;
            _queryService.NotebookToQuery = _dataService.CurrentNotebook;
            _queryService.LoadQueryablePages();
            SetPageRangeToAllPages();
            _queryService.StudentIDsToQuery = _queryService.RosterToQuery.ListOfStudents.Select(s => s.ID).ToList();

            _queryService.LoadSavedQueries();
            SavedQueries = _queryService.SavedQueries;
        }

        private void SetPageRangeToAllPages()
        {
            var cacheFilePath = _queryService.NotebookToQuery.ContainerZipFilePath;
            using (var zip = ZipFile.Read(cacheFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var pageNumbers = DataService.GetAllPageNumbersInNotebook(zip, _queryService.NotebookToQuery);
                _queryService.PageNumbersToQuery = pageNumbers;
            }
        }

        #endregion // Methods

        #region Commands

        private void InitializeCommands()
        {
            ToggleConditionalCommand = new Command(OnToggleConditionalCommandExecute);

            SelectPageRangeCommand = new Command(OnSelectPageRangeCommandExecute);


            SaveQueryCommand = new Command(OnSaveQueryCommandExecute);
            RunQueryCommand = new Command(OnRunQueryCommandExecute);
            SetCurrentPageCommand = new Command<QueryService.QueryResult>(OnSetCurrentPageCommandExecute);
        }

        public Command ToggleConditionalCommand { get; private set; }

        private void OnToggleConditionalCommandExecute()
        {
            switch (CurrentCodeQuery.Conditional)
            {
                case QueryConditionals.None:
                    CurrentCodeQuery.Conditional = QueryConditionals.And;
                    break;
                case QueryConditionals.And:
                    CurrentCodeQuery.Conditional = QueryConditionals.Or;
                    break;
                case QueryConditionals.Or:
                    CurrentCodeQuery.Conditional = QueryConditionals.None;
                    break;
            }
        }

        /// <summary>Selects which pages to run the query on.</summary>
        public Command SelectPageRangeCommand { get; private set; }

        private void OnSelectPageRangeCommandExecute()
        {
            var textInputViewModel = new TextInputViewModel
                                     {
                                         TextPrompt = "Enter page range or leave blank for all pages.",
                                         InputText = RangeHelper.ParseIntNumbersToString(_queryService.PageNumbersToQuery, true, true)
                                     };
            var textInputView = new TextInputView(textInputViewModel);
            textInputView.ShowDialog();

            if (textInputView.DialogResult == null ||
                textInputView.DialogResult != true ||
                string.IsNullOrEmpty(textInputViewModel.InputText))
            {
                SetPageRangeToAllPages();
                return;
            }

            var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(textInputViewModel.InputText).ToList();
            if (!pageNumbersToOpen.Any())
            {
                SetPageRangeToAllPages();
                return;
            }

            _queryService.PageNumbersToQuery = pageNumbersToOpen;
        }

        public Command SaveQueryCommand { get; private set; }

        private void OnSaveQueryCommandExecute()
        {
            if (string.IsNullOrWhiteSpace(CurrentCodeQuery.QueryName))
            {
                var numberOfQueries = SavedQueries.SavedQueries.Count + 1;
                CurrentCodeQuery.QueryName = $"Q{numberOfQueries}";
            }

            if (!SavedQueries.SavedQueries.Contains(CurrentCodeQuery))
            {
                SavedQueries.SavedQueries.Add(CurrentCodeQuery);
            }

            DataService.SaveQueries(SavedQueries);
        }

        /// <summary>Runs a query using the current QueryString.</summary>
        public Command RunQueryCommand { get; private set; }

        private void OnRunQueryCommandExecute()
        {
            QueryResults.Clear();

            var queryResults = _queryService.RunQuery(CurrentCodeQuery);
            queryResults = queryResults.OrderBy(q => q.PageNumber).ThenBy(q => q.StudentName).ToList();
            if (!queryResults.Any())
            {
                MessageBox.Show("No results found.");
            }
            QueryResults = queryResults.ToObservableCollection();
        }

        public Command<QueryService.QueryResult> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(QueryService.QueryResult queryResult)
        {
            var page = _dataService.GetPageByCompositeID(queryResult.Page.PageNameComposite, queryResult.Page.StudentID);
            if (page == null)
            {
                return;
            }

            _dataService.SetCurrentPage(page);
        }

        #endregion // Commands
    }
}
﻿using System;
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

        #region Methods

        private void InitializeQueryConstraints()
        {
            _queryService.RosterToQuery = _dataService.CurrentClassRoster;
            _queryService.NotebookToQuery = _dataService.CurrentNotebook;
            SetPageRangeToAllPages();
            _queryService.StudentIDsToQuery = _queryService.RosterToQuery.ListOfStudents.Select(s => s.ID).ToList();
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
            SelectCacheCommand = new Command(OnSelectCacheCommandExecute);
            SelectPageRangeCommand = new Command(OnSelectPageRangeCommandExecute);
            SelectStudentsCommand = new Command(OnSelectStudentsCommandExecute);

            RunQueryCommand = new Command(OnRunQueryCommandExecute);
            ShowReportsCommand = new TaskCommand(OnShowReportsCommandExecuteAsync);
        }

        /// <summary>Selects which cache to run the query on.</summary>
        public Command SelectCacheCommand { get; private set; }

        private void OnSelectCacheCommandExecute()
        {
            // TODO
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

        /// <summary>Selects which students to run the query on.</summary>
        public Command SelectStudentsCommand { get; private set; }

        private void OnSelectStudentsCommandExecute()
        {

        }

        /// <summary>Runs a query using the current QueryString.</summary>
        public Command RunQueryCommand { get; private set; }

        private void OnRunQueryCommandExecute()
        {
            QueryResults.Clear();
            var queryResults = _queryService.RunQuery(QueryString).OrderBy(q => q.PageNumber).ThenBy(q => q.StudentName);
            QueryResults = queryResults.Select(r => $"Page {r.PageNumber}, {r.StudentName}\n - {string.Join("\n - ", r.MatchingQueryCodes.Select(q => q.FormattedValue))}").ToObservableCollection();
        }

        /// <summary>Runs a query using the current QueryString.</summary>
        public TaskCommand ShowReportsCommand { get; private set; }

        private async Task OnShowReportsCommandExecuteAsync()
        {
            var report = _queryService.GatherReports();
            var viewModel = new ReportsViewModel(report);
            await viewModel.ShowWindowAsDialogAsync();
        }

        #endregion // Commands
    }
}
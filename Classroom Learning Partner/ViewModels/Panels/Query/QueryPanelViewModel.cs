﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using Catel;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum GroupTypes
    {
        StudentName,
        PageNumber,
        RepresentationType,
        OverallCorrectness,
        Cluster
    }

    public class QueryPanelViewModel : APanelBaseViewModel
    {
        private static readonly PropertyGroupDescription PageNumberGroup = new PropertyGroupDescription("PageNumber");
        private static readonly PropertyGroupDescription StudentNameGroup = new PropertyGroupDescription("StudentName");
        private static readonly PropertyGroupDescription ClusterGroup = new PropertyGroupDescription("Cluster");

        private static readonly SortDescription PageNumberAscendingSort = new SortDescription("PageNumber", ListSortDirection.Ascending);
        private static readonly SortDescription StudentNameAscendingSort = new SortDescription("StudentName", ListSortDirection.Ascending);
        private static readonly SortDescription ClusterAscendingSort = new SortDescription("Cluster", ListSortDirection.Ascending);

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

            GroupedQueryResults.Source = QueryResults;
            CurrentCodeQuery = new AnalysisCodeQuery();
            CurrentGroupType = GroupTypes.StudentName;
        }

        #region Events

        private async Task QueryPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
            MinLength = 450.0;
        }

        #endregion // Events

        #region Model

        [Model(SupportIEditableObject = false)]
        public AnalysisCodeQuery CurrentCodeQuery
        {
            get => GetValue<AnalysisCodeQuery>(CurrentCodeQueryProperty);
            set => SetValue(CurrentCodeQueryProperty, value);
        }

        public static readonly PropertyData CurrentCodeQueryProperty = RegisterProperty(nameof(CurrentCodeQuery), typeof(AnalysisCodeQuery));

        [ViewModelToModel("CurrentCodeQuery")]
        public ObservableCollection<QueryCondition> Conditions
        {
            get => GetValue<ObservableCollection<QueryCondition>>(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
        }

        public static readonly PropertyData ConditionsProperty = RegisterProperty(nameof(Conditions), typeof(ObservableCollection<QueryCondition>));


        [ViewModelToModel("CurrentCodeQuery")]
        public QueryConditionals Conditional
        {
            get => GetValue<QueryConditionals>(ConditionalProperty);
            set => SetValue(ConditionalProperty, value);
        }

        public static readonly PropertyData ConditionalProperty = RegisterProperty(nameof(Conditional), typeof(QueryConditionals));

        #endregion // Model

        #region Bindings

        public AnalysisCodeQuery CurrentSavedQuery
        {
            get => GetValue<AnalysisCodeQuery>(CurrentSavedQueryProperty);
            set => SetValue(CurrentSavedQueryProperty, value);
        }

        public static readonly PropertyData CurrentSavedQueryProperty = RegisterProperty(nameof(CurrentSavedQuery), typeof(AnalysisCodeQuery));

        public CollectionViewSource GroupedQueryResults
        {
            get => GetValue<CollectionViewSource>(GroupedQueryResultsProperty);
            set => SetValue(GroupedQueryResultsProperty, value);
        }

        public static readonly PropertyData GroupedQueryResultsProperty =
            RegisterProperty(nameof(GroupedQueryResults), typeof(CollectionViewSource), () => new CollectionViewSource());

        public GroupTypes CurrentGroupType
        {
            get => GetValue<GroupTypes>(CurrentGroupTypeProperty);
            set
            {
                SetValue(CurrentGroupTypeProperty, value);
                ApplySortAndGroup();
            }
        }

        public static readonly PropertyData CurrentGroupTypeProperty = RegisterProperty(nameof(CurrentGroupType), typeof(GroupTypes));

        public bool IsCurrentQuerySaved
        {
            get => GetValue<bool>(IsCurrentQuerySavedProperty);
            set => SetValue(IsCurrentQuerySavedProperty, value);
        }

        public static readonly PropertyData IsCurrentQuerySavedProperty = RegisterProperty(nameof(IsCurrentQuerySaved), typeof(bool), false);

        public override double InitialLength => 750.0;

        public Queries SavedQueries
        {
            get => GetValue<Queries>(SavedQueriesProperty);
            set => SetValue(SavedQueriesProperty, value);
        }

        public static readonly PropertyData SavedQueriesProperty = RegisterProperty(nameof(SavedQueries), typeof(Queries), null);

        public ObservableCollection<QueryService.QueryResult> QueryResults
        {
            get => GetValue<ObservableCollection<QueryService.QueryResult>>(QueryResultsProperty);
            set => SetValue(QueryResultsProperty, value);
        }

        public static readonly PropertyData QueryResultsProperty =
            RegisterProperty(nameof(QueryResults), typeof(ObservableCollection<QueryService.QueryResult>), () => new ObservableCollection<QueryService.QueryResult>());

        public QueryService.QueryResult SelectedQueryResult
        {
            get => GetValue<QueryService.QueryResult>(SelectedQueryResultProperty);
            set => SetValue(SelectedQueryResultProperty, value);
        }

        public static readonly PropertyData SelectedQueryResultProperty = RegisterProperty(nameof(SelectedQueryResult), typeof(QueryService.QueryResult), null);

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
            SelectPageRangeCommand = new Command(OnSelectPageRangeCommandExecute);
            SetANDConditionalCommand = new Command(OnSetANDConditionalCommandExecute);
            SetORConditionalCommand = new Command(OnSetORConditionalCommandExecute);
            RemoveConditionCommand = new Command<QueryCondition>(OnRemoveConditionCommandExecute);

            SaveQueryCommand = new Command(OnSaveQueryCommandExecute);
            SelectSavedQueryCommand = new Command<AnalysisCodeQuery>(OnSelectSavedQueryCommandExecute);
            EditSavedQueryCommand = new Command<AnalysisCodeQuery>(OnEditSavedQueryCommandExecute);
            DeleteSavedQueryCommand = new Command<AnalysisCodeQuery>(OnDeleteSavedQueryCommandExecute);

            NewQueryCommand = new Command(OnNewQueryCommandExecute);
            
            RunQueryCommand = new Command(OnRunQueryCommandExecute);
            ClusterCommand = new Command(OnClusterCommandExecute);
            SetCurrentPageCommand = new Command<QueryService.QueryResult>(OnSetCurrentPageCommandExecute);
        }

        public Command SetANDConditionalCommand { get; private set; }

        private void OnSetANDConditionalCommandExecute()
        {
            CurrentCodeQuery.Conditional = QueryConditionals.And;
            CurrentCodeQuery.Conditions.Add(new QueryCondition());
        }

        public Command SetORConditionalCommand { get; private set; }

        private void OnSetORConditionalCommandExecute()
        {
            CurrentCodeQuery.Conditional = QueryConditionals.Or;
            CurrentCodeQuery.Conditions.Add(new QueryCondition());
        }

        public Command<QueryCondition> RemoveConditionCommand { get; private set; }

        private void OnRemoveConditionCommandExecute(QueryCondition condition)
        {
            var conditionIndex = CurrentCodeQuery.Conditions.IndexOf(condition);
            if (conditionIndex == 0)
            {
                CurrentCodeQuery.Conditions[0] = new QueryCondition();
            }
            else if (conditionIndex == 1)
            {
                CurrentCodeQuery.Conditional = QueryConditionals.None;
                CurrentCodeQuery.Conditions.Remove(condition);
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
            if (SavedQueries.SavedQueries.Contains(CurrentCodeQuery))
            {
                return;
            }

            if (CurrentCodeQuery.Conditions.Any(c => c.QueryPart is null))
            {
                MessageBox.Show("Query must be fully filled out before saving.");
                return;
            }

            var numberOfQueries = SavedQueries.AutoQueryCount;
            SavedQueries.AutoQueryCount++;
            CurrentCodeQuery.QueryName = $"Q{numberOfQueries}";
            SavedQueries.SavedQueries.Add(CurrentCodeQuery);
            DataService.SaveQueries(SavedQueries);
            CurrentSavedQuery = CurrentCodeQuery;
            CurrentCodeQuery = null;
            IsCurrentQuerySaved = true;
        }

        public Command<AnalysisCodeQuery> SelectSavedQueryCommand { get; private set; }

        private void OnSelectSavedQueryCommandExecute(AnalysisCodeQuery query)
        {
            CurrentCodeQuery = null;
            CurrentSavedQuery = query;
            IsCurrentQuerySaved = true;
        }

        public Command<AnalysisCodeQuery> EditSavedQueryCommand { get; private set; }

        private void OnEditSavedQueryCommandExecute(AnalysisCodeQuery query)
        {
            var analysisCodeCopy = query.DeepCopy();
            // TODO: try something different from DeepCopy?
            analysisCodeCopy.QueryName = string.Empty;
            CurrentCodeQuery = null;
            CurrentCodeQuery = analysisCodeCopy;
            
            IsCurrentQuerySaved = false;
            CurrentSavedQuery = null;
        }

        public Command<AnalysisCodeQuery> DeleteSavedQueryCommand { get; private set; }

        private void OnDeleteSavedQueryCommandExecute(AnalysisCodeQuery query)
        {
            SavedQueries.SavedQueries.Remove(query);
            if (!SavedQueries.SavedQueries.Any())
            {
                SavedQueries.AutoQueryCount = 1;
            }
            DataService.SaveQueries(SavedQueries);
            CurrentCodeQuery = null;
            CurrentCodeQuery = new AnalysisCodeQuery();

            IsCurrentQuerySaved = false;
            CurrentSavedQuery = null;
        }

        public Command NewQueryCommand { get; private set; }

        private void OnNewQueryCommandExecute()
        {
            CurrentCodeQuery = null;
            CurrentCodeQuery = new AnalysisCodeQuery();

            IsCurrentQuerySaved = false;
            CurrentSavedQuery = null;
        }

        public Command RunQueryCommand { get; private set; }

        private void OnRunQueryCommandExecute()
        {
            var codeToQuery = CurrentCodeQuery ?? CurrentSavedQuery;
            if (codeToQuery.Conditions.Any(c => c.QueryPart is null))
            {
                MessageBox.Show("Query must be fully filled out first.");
                return;
            }

            QueryResults.Clear();

            var queryResults = _queryService.RunQuery(codeToQuery);
            queryResults = queryResults.OrderBy(q => q.PageNumber).ThenBy(q => q.StudentName).ToList();
            if (!queryResults.Any())
            {
                MessageBox.Show("No results found.");
            }
            QueryResults.AddRange(queryResults);
        }

        public Command ClusterCommand { get; private set; }

        private void OnClusterCommandExecute()
        {
            var queryablePages = QueryResults.Select(qr => qr.Page).ToList();
            if (!queryablePages.Any())
            {
                queryablePages = _queryService.QueryablePages.Where(qp => _queryService.PageNumbersToQuery.Contains(qp.PageNameComposite.PageNumber)).ToList();
            }

            var queryResults = _queryService.Cluster(queryablePages);
            queryResults = queryResults.OrderBy(q => q.PageNumber).ThenBy(q => q.StudentName).ToList();

            QueryResults.Clear();
            QueryResults.AddRange(queryResults);
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

        #region Sorts

        public void ApplySortAndGroup()
        {
            switch(CurrentGroupType)
            {
                case GroupTypes.StudentName:
                    ApplySortAndGroupByName();
                    break;
                case GroupTypes.PageNumber:
                    ApplySortAndGroupByPageNumber();
                    break;
                case GroupTypes.Cluster:
                    ApplySortAndGroupByCluster();
                    break;
                default:
                    ApplySortAndGroupByName();
                    break;
            }
        }

        public void ApplySortAndGroupByName()
        {
            GroupedQueryResults.GroupDescriptions.Clear();
            GroupedQueryResults.SortDescriptions.Clear();

            GroupedQueryResults.GroupDescriptions.Add(StudentNameGroup);
            GroupedQueryResults.SortDescriptions.Add(StudentNameAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(PageNumberAscendingSort);
        }

        public void ApplySortAndGroupByPageNumber()
        {
            GroupedQueryResults.GroupDescriptions.Clear();
            GroupedQueryResults.SortDescriptions.Clear();

            GroupedQueryResults.GroupDescriptions.Add(PageNumberGroup);
            GroupedQueryResults.SortDescriptions.Add(PageNumberAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(StudentNameAscendingSort);
        }

        public void ApplySortAndGroupByCluster()
        {
            GroupedQueryResults.GroupDescriptions.Clear();
            GroupedQueryResults.SortDescriptions.Clear();

            GroupedQueryResults.GroupDescriptions.Add(ClusterGroup);
            GroupedQueryResults.SortDescriptions.Add(ClusterAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(PageNumberAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(StudentNameAscendingSort);
        }

        #endregion // Sorts
    }
}
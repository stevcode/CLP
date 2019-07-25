using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
        ClusterName,
        ClusterSize
    }

    public class QueryPanelViewModel : APanelBaseViewModel
    {
        private static readonly PropertyGroupDescription PageNumberGroup = new PropertyGroupDescription("PageNumber");
        private static readonly PropertyGroupDescription StudentNameGroup = new PropertyGroupDescription("StudentName");
        private static readonly PropertyGroupDescription ClusterNameGroup = new PropertyGroupDescription("ClusterName");

        private static readonly SortDescription PageNumberAscendingSort = new SortDescription("PageNumber", ListSortDirection.Ascending);
        private static readonly SortDescription StudentNameAscendingSort = new SortDescription("StudentName", ListSortDirection.Ascending);
        private static readonly SortDescription ClusterNameAscendingSort = new SortDescription("ClusterName", ListSortDirection.Ascending);
        private static readonly SortDescription ClusterSizeDescendingSort = new SortDescription("ClusterSize", ListSortDirection.Descending);

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

        public ObservableCollection<QueryResult> QueryResults
        {
            get => GetValue<ObservableCollection<QueryResult>>(QueryResultsProperty);
            set => SetValue(QueryResultsProperty, value);
        }

        public static readonly PropertyData QueryResultsProperty =
            RegisterProperty(nameof(QueryResults), typeof(ObservableCollection<QueryResult>), () => new ObservableCollection<QueryResult>());

        public QueryResult SelectedQueryResult
        {
            get => GetValue<QueryResult>(SelectedQueryResultProperty);
            set => SetValue(SelectedQueryResultProperty, value);
        }

        public static readonly PropertyData SelectedQueryResultProperty = RegisterProperty(nameof(SelectedQueryResult), typeof(QueryResult), null);

        public bool IsGeneratingReportOnQuery
        {
            get => GetValue<bool>(IsGeneratingReportOnQueryProperty);
            set => SetValue(IsGeneratingReportOnQueryProperty, value);
        }

        public static readonly PropertyData IsGeneratingReportOnQueryProperty = RegisterProperty(nameof(IsGeneratingReportOnQuery), typeof(bool), false);

        public string PagesFilter
        {
            get => GetValue<string>(PagesFilterProperty);
            set => SetValue(PagesFilterProperty, value);
        }

        public static readonly PropertyData PagesFilterProperty = RegisterProperty(nameof(PagesFilter), typeof(string), string.Empty);

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

            PagesFilter = RangeHelper.ParseIntNumbersToString(_queryService.PageNumbersToQuery.OrderBy(i => i), true, true);
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
            SetCurrentPageCommand = new Command<QueryResult>(OnSetCurrentPageCommandExecute);
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
                                         InputText = RangeHelper.ParseIntNumbersToString(_queryService.PageNumbersToQuery.OrderBy(i => i), true, true)
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
            PagesFilter = textInputViewModel.InputText;
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

            if (CurrentGroupType == GroupTypes.ClusterName ||
                CurrentGroupType == GroupTypes.ClusterSize)
            {
                CurrentGroupType = GroupTypes.StudentName;
            }

            if (!IsGeneratingReportOnQuery)
            {
                return;
            }

            const string FOLDER_NAME = "CLP Reports";
            var folderPath = Path.Combine(DataService.DesktopFolderPath, FOLDER_NAME);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            const string FILE_EXTENSION = "txt";
            var queryIdentifierName = codeToQuery.LongFormattedValue.Replace(':', '-').Substring(0, 25);
            var fileName = $"Query Report - {DateTime.Now:yy.MM.dd-h.mm.ss} - {queryIdentifierName}.{FILE_EXTENSION}";
            var filePath = Path.Combine(folderPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.AppendAllText(filePath, "*****QUERY REPORT*****\n\n");
            File.AppendAllText(filePath, $"Query: {codeToQuery.LongFormattedValue}\n\n");

            foreach (var queryResult in QueryResults.OrderBy(qr => qr.StudentName).ThenBy(qr => qr.PageNumber))
            {
                File.AppendAllText(filePath, $"{queryResult.FormattedValue}\n\n");
            }
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

            if (CurrentGroupType == GroupTypes.ClusterName)
            {
                return;
            }

            CurrentGroupType = GroupTypes.ClusterSize;

            if (!IsGeneratingReportOnQuery)
            {
                return;
            }

            const string FOLDER_NAME = "CLP Reports";
            var folderPath = Path.Combine(DataService.DesktopFolderPath, FOLDER_NAME);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            const string FILE_EXTENSION = "txt";
            var timeNow = DateTime.Now;
            var fileName = $"Cluster Report - {timeNow:yy.MM.dd-h.mm.ss}.{FILE_EXTENSION}";
            var filePath = Path.Combine(folderPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.AppendAllText(filePath, "*****CLUSTER REPORT*****\n\n");

            foreach (var queryGroup in QueryResults.GroupBy(qr => qr.ClusterName).OrderBy(g => g.Key))
            {
                var clusterName = queryGroup.Key;
                File.AppendAllText(filePath, $"Cluster Name: {clusterName}\n\n");
                foreach (var queryResult in queryGroup.OrderBy(p => p.PageNumber))
                {
                    File.AppendAllText(filePath, $"{queryResult.FormattedValue}\n\n");
                }
            }

            var fileName2 = $"Cluster Report (Distances) - {timeNow:yy.MM.dd-h.mm.ss}.{FILE_EXTENSION}";
            var filePath2 = Path.Combine(folderPath, fileName2);
            if (File.Exists(filePath2))
            {
                File.Delete(filePath2);
            }

            File.AppendAllText(filePath2, "*****CLUSTER REPORT (DISTANCES)*****\n\n");

            foreach (var queryGroup in QueryResults.GroupBy(qr => qr.ClusterName).OrderBy(g => g.Key))
            {
                var clusterName = queryGroup.Key;
                File.AppendAllText(filePath2, $"Cluster Name: {clusterName}\n\n");
                foreach (var queryResult in queryGroup.OrderBy(p => p.PageNumber))
                {
                    File.AppendAllText(filePath2, $"{queryResult.StudentName}, Page {queryResult.PageNumber}\n{queryResult.Page.FormattedDistance}\n\n");
                }
            }
        }

        public Command<QueryResult> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(QueryResult queryResult)
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
                case GroupTypes.ClusterName:
                    ApplySortAndGroupByClusterName();
                    break;
                case GroupTypes.ClusterSize:
                    ApplySortAndGroupByClusterSize();
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

        public void ApplySortAndGroupByClusterName()
        {
            GroupedQueryResults.GroupDescriptions.Clear();
            GroupedQueryResults.SortDescriptions.Clear();

            GroupedQueryResults.GroupDescriptions.Add(ClusterNameGroup);
            GroupedQueryResults.SortDescriptions.Add(ClusterNameAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(PageNumberAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(StudentNameAscendingSort);
        }

        public void ApplySortAndGroupByClusterSize()
        {
            GroupedQueryResults.GroupDescriptions.Clear();
            GroupedQueryResults.SortDescriptions.Clear();

            GroupedQueryResults.GroupDescriptions.Add(ClusterNameGroup);
            GroupedQueryResults.SortDescriptions.Add(ClusterSizeDescendingSort);
            GroupedQueryResults.SortDescriptions.Add(PageNumberAscendingSort);
            GroupedQueryResults.SortDescriptions.Add(StudentNameAscendingSort);
        }

        #endregion // Sorts
    }
}
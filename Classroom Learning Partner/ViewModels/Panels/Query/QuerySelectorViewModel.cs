using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum Stages
    {
        Prompt0,
        Condition1,
        Query2,
        Constraint2,
        Button3
    }

    public class QuerySelectorViewModel : ViewModelBase
    {
        private readonly IQueryService _queryService;

        public QuerySelectorViewModel(QueryCondition condition, IQueryService queryService)
        {
            Argument.IsNotNull(() => queryService);

            _queryService = queryService;

            Condition = condition;
            
            InitializedAsync += QuerySelectorViewModel_InitializedAsync;
            
            InitializeCommands();
        }

        #region Events

        private async Task QuerySelectorViewModel_InitializedAsync(object sender, EventArgs e)
        {
            if (QueryPart != null)
            {
                CurrentStage = Stages.Button3;
            }
        }

        #endregion // Events

        #region Model

        [Model(SupportIEditableObject = false)]
        public QueryCondition Condition
        {
            get => GetValue<QueryCondition>(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        public static readonly PropertyData ConditionProperty = RegisterProperty(nameof(Condition), typeof(QueryCondition));

        [ViewModelToModel("Condition")]
        public IQueryPart QueryPart
        {
            get => GetValue<IQueryPart>(QueryPartProperty);
            set => SetValue(QueryPartProperty, value);
        }

        public static readonly PropertyData QueryPartProperty = RegisterProperty(nameof(QueryPart), typeof(IQueryPart), null, OnQueryPartChanged);

        private static void OnQueryPartChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            if (!(sender is QuerySelectorViewModel querySelectorViewModel))
            {
                return;
            }

            if (advancedPropertyChangedEventArgs.NewValue is null)
            {
                querySelectorViewModel.CurrentStage = Stages.Prompt0;
            }
        }

        #endregion // Model

        #region Bindings

        public Stages CurrentStage
        {
            get => GetValue<Stages>(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public static readonly PropertyData CurrentStageProperty = RegisterProperty(nameof(CurrentStage), typeof(Stages), Stages.Prompt0);

        public ObservableCollection<AnalysisCodeQuery> SavedQueries => _queryService?.SavedQueries.SavedQueries;

        public List<IAnalysisCode> AvailableConditions => AnalysisCode.GenerateAvailableQueryConditions().ToList();

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            AdvanceToConditionStageCommand = new Command(OnAdvanceToConditionStageCommandExecute);
            AdvanceToSavedQueriesStageCommand = new Command(OnAdvanceToSavedQueriesStageCommandExecute);
            SelectSavedQueryCommand = new Command<AnalysisCodeQuery>(OnSelectSavedQueryCommandExecute);
            SelectConditionCommand = new Command<IQueryPart>(OnSelectConditionCommandExecute);
        }

        public Command AdvanceToConditionStageCommand { get; private set; }

        private void OnAdvanceToConditionStageCommandExecute()
        {
            CurrentStage = Stages.Condition1;
        }

        public Command AdvanceToSavedQueriesStageCommand { get; private set; }

        private void OnAdvanceToSavedQueriesStageCommandExecute()
        {
            CurrentStage = Stages.Query2;
        }

        public Command<AnalysisCodeQuery> SelectSavedQueryCommand { get; private set; }

        private void OnSelectSavedQueryCommandExecute(AnalysisCodeQuery query)
        {
            QueryPart = query;
            CurrentStage = Stages.Button3;
        }

        public Command<IQueryPart> SelectConditionCommand { get; private set; }

        private void OnSelectConditionCommandExecute(IQueryPart queryPart)
        {
            QueryPart = queryPart;
            CurrentStage = Stages.Constraint2;
        }

        #endregion // Commands
    }
}
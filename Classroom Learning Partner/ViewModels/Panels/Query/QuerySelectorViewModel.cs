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

    public enum ConditionPlaces
    {
        First,
        Second
    }

    public class QuerySelectorViewModel : ViewModelBase
    {
        private readonly IQueryService _queryService;

        public QuerySelectorViewModel(AnalysisCodeQuery query, IQueryService queryService)
        {
            Argument.IsNotNull(() => queryService);

            _queryService = queryService;

            Query = query;
            
            InitializedAsync += QuerySelectorViewModel_InitializedAsync;
            
            InitializeCommands();
        }

        #region Events

        private async Task QuerySelectorViewModel_InitializedAsync(object sender, EventArgs e)
        {
            if (Query.FirstCondition != null)
            {
                CurrentStage = Stages.Button3;
                switch (ConditionPlace)
                {
                    case ConditionPlaces.First:
                        SelectedIQueryPart = Query.FirstCondition;
                        break;
                    case ConditionPlaces.Second:
                        SelectedIQueryPart = Query.SecondCondition;
                        break;
                }
            }
        }

        #endregion // Events

        #region Model

        [Model(SupportIEditableObject = false)]
        public AnalysisCodeQuery Query
        {
            get => GetValue<AnalysisCodeQuery>(QueryProperty);
            set => SetValue(QueryProperty, value);
        }

        public static readonly PropertyData QueryProperty = RegisterProperty(nameof(Query), typeof(AnalysisCodeQuery));

        #endregion // Model

        #region Bindings

        /// <summary>Mapped from View</summary>
        public ConditionPlaces ConditionPlace
        {
            get => GetValue<ConditionPlaces>(ConditionPlaceProperty);
            set => SetValue(ConditionPlaceProperty, value);
        }

        public static readonly PropertyData ConditionPlaceProperty = RegisterProperty(nameof(ConditionPlace), typeof(ConditionPlaces));

        public Stages CurrentStage
        {
            get => GetValue<Stages>(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public static readonly PropertyData CurrentStageProperty = RegisterProperty(nameof(CurrentStage), typeof(Stages), Stages.Prompt0);

        public ObservableCollection<AnalysisCodeQuery> SavedQueries => _queryService?.SavedQueries.SavedQueries;

        public IQueryPart SelectedIQueryPart
        {
            get => GetValue<IQueryPart>(SelectedIQueryPartProperty);
            set => SetValue(SelectedIQueryPartProperty, value);
        }

        public static readonly PropertyData SelectedIQueryPartProperty = RegisterProperty(nameof(SelectedIQueryPart), typeof(IQueryPart), null);

        public List<IAnalysisCode> AvailableConditions => AnalysisCode.GenerateAvailableQueryConditions().ToList();

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            AdvanceToConditionStageCommand = new Command(OnAdvanceToConditionStageCommandExecute);
            AdvanceToSavedQueriesStageCommand = new Command(OnAdvanceToSavedQueriesStageCommandExecute);
            SelectSavedQueryCommand = new Command<AnalysisCodeQuery>(OnSelectSavedQueryCommandExecute);
            SelectConditionCommand = new Command<IQueryPart>(OnSelectConditionCommandExecute);
            FinalizeConstraintsCommand = new Command(OnFinalizeConstraintsCommandExecute);

            ClearStageCommand = new Command(OnClearStageCommandExecute);
        }

        public Command AdvanceToConditionStageCommand { get; private set; }

        private void OnAdvanceToConditionStageCommandExecute()
        {
            if (!string.IsNullOrWhiteSpace(Query.QueryName))
            {
                return;
            }

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
            SelectedIQueryPart = query;
            switch (ConditionPlace)
            {
                case ConditionPlaces.First:
                    Query.FirstCondition = query;
                    break;
                case ConditionPlaces.Second:
                    Query.SecondCondition = query;
                    break;
            }

            CurrentStage = Stages.Button3;
        }

        public Command<IQueryPart> SelectConditionCommand { get; private set; }

        private void OnSelectConditionCommandExecute(IQueryPart queryPart)
        {
            SelectedIQueryPart = queryPart;
            switch (ConditionPlace)
            {
                case ConditionPlaces.First:
                    Query.FirstCondition = queryPart;
                    break;
                case ConditionPlaces.Second:
                    Query.SecondCondition = queryPart;
                    break;
            }

            CurrentStage = Stages.Constraint2;
        }

        public Command FinalizeConstraintsCommand { get; private set; }

        private void OnFinalizeConstraintsCommandExecute()
        {
            (SelectedIQueryPart as AnalysisCode).Hack = "Blah";
            CurrentStage = Stages.Button3;
        }

        public Command ClearStageCommand { get; private set; }

        private void OnClearStageCommandExecute()
        {
            SelectedIQueryPart = null;
            switch (ConditionPlace)
            {
                case ConditionPlaces.First:
                    Query.FirstCondition = null;
                    break;
                case ConditionPlaces.Second:
                    Query.SecondCondition = null;
                    break;
            }
            CurrentStage = Stages.Prompt0;
        }

        #endregion // Commands
    }
}
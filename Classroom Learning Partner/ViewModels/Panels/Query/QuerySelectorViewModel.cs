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

        public QuerySelectorViewModel(IQueryService queryService)
        {
            Argument.IsNotNull(() => queryService);

            _queryService = queryService;
            
            InitializeCommands();
        }

        #region Bindings

        public List<string> SavedQueries =>
            new List<string>
            {
                "Q1",
                "Q2",
                "Q3"
            };

        public Stages CurrentStage
        {
            get => GetValue<Stages>(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public static readonly PropertyData CurrentStageProperty = RegisterProperty(nameof(CurrentStage), typeof(Stages), Stages.Prompt0);

        public ConditionPlaces CurrentConditionPlace
        {
            get => GetValue<ConditionPlaces>(CurrentConditionPlaceProperty);
            set => SetValue(CurrentConditionPlaceProperty, value);
        }

        public static readonly PropertyData CurrentConditionPlaceProperty = RegisterProperty(nameof(CurrentConditionPlace), typeof(ConditionPlaces), ConditionPlaces.First);

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
            SelectSavedQueryCommand = new Command<string>(OnSelectSavedQueryCommandExecute);
            SelectConditionCommand = new Command<IQueryPart>(OnSelectConditionCommandExecute);
            FinalizeConstraintsCommand = new Command(OnFinalizeConstraintsCommandExecute);


            ClearStageCommand = new Command(OnClearStageCommandExecute);
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

        public Command<string> SelectSavedQueryCommand { get; private set; }

        private void OnSelectSavedQueryCommandExecute(string queryName)
        {
            var temp = new AnalysisCodeQuery();
            temp.QueryName = queryName;
            SelectedIQueryPart = temp;
            CurrentStage = Stages.Button3;
        }

        public Command<IQueryPart> SelectConditionCommand { get; private set; }

        private void OnSelectConditionCommandExecute(IQueryPart queryPart)
        {
            SelectedIQueryPart = queryPart;
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
            CurrentStage = Stages.Prompt0;
        }

        #endregion // Commands
    }
}
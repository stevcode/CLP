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

        public QuerySelectorViewModel(IQueryService queryService)
        {
            Argument.IsNotNull(() => queryService);

            _queryService = queryService;
            
            InitializeCommands();
        }

        #region Bindings

        public Stages CurrentStage
        {
            get => GetValue<Stages>(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public static readonly PropertyData CurrentStageProperty = RegisterProperty(nameof(CurrentStage), typeof(Stages), Stages.Prompt0);

        /// <summary></summary>
        public IQueryPart SelectedIQueryPart
        {
            get => GetValue<IQueryPart>(SelectedIQueryPartProperty);
            set => SetValue(SelectedIQueryPartProperty, value);
        }

        public static readonly PropertyData SelectedIQueryPartProperty = RegisterProperty(nameof(SelectedIQueryPart), typeof(IQueryPart), null);

        public List<QueryCondition> AvailableConditions => QueryCondition.GenerateAvailableQueryConditions().ToList();

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            AdvanceToConditionStageCommand = new Command(OnAdvanceToConditionStageCommandExecute);
            SelectConditionOrQueryCommand = new Command<IQueryPart>(OnSelectConditionOrQueryCommandExecute);


            ClearStageCommand = new Command(OnClearStageCommandExecute);
        }

        public Command AdvanceToConditionStageCommand { get; private set; }

        private void OnAdvanceToConditionStageCommandExecute()
        {
            CurrentStage = Stages.Condition1;
        }

        public Command<IQueryPart> SelectConditionOrQueryCommand { get; private set; }

        private void OnSelectConditionOrQueryCommandExecute(IQueryPart queryPart)
        {
            SelectedIQueryPart = queryPart;
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
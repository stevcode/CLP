using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class AdditionRelationDefinitionTagViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="AdditionRelationDefinitionTagViewModel" /> class.</summary>
        public AdditionRelationDefinitionTagViewModel(AdditionRelationDefinitionTag additionRelationDefinition)
        {
            Model = additionRelationDefinition;
            InitializeCommands();

            Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
            Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        #endregion //Constructor

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public AdditionRelationDefinitionTag Model
        {
            get { return GetValue<AdditionRelationDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(AdditionRelationDefinitionTag));

        /// <summary>Type of addition relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public AdditionRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<AdditionRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(AdditionRelationDefinitionTag.RelationTypes));

        /// <summary>Value of the Sum.</summary>
        [ViewModelToModel("Model")]
        public double Sum
        {
            get { return GetValue<double>(SumProperty); }
            set { SetValue(SumProperty, value); }
        }

        public static readonly PropertyData SumProperty = RegisterProperty("Sum", typeof(double));

        #endregion //Model

        #region Bindings

        /// <summary>List of all the addends in the addition relation.</summary>
        public ObservableCollection<IRelationPart> Addends
        {
            get { return GetValue<ObservableCollection<IRelationPart>>(AddendsProperty); }
            set { SetValue(AddendsProperty, value); }
        }

        public static readonly PropertyData AddendsProperty = RegisterProperty("Addends", typeof(ObservableCollection<IRelationPart>), () => new ObservableCollection<IRelationPart>());

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            AddAddendCommand = new Command(OnAddAddendCommandExecute);
            TagCommand = new Command<IRelationPart>(OnTagCommandExecute);
            UntagCommand = new Command<IRelationPart>(OnUntagCommandExecute);
            CalculateSumCommand = new Command(OnCalculateSumCommandExecute);
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Adds a zero value addend to the relation definition.</summary>
        public Command AddAddendCommand { get; private set; }

        private void OnAddAddendCommandExecute()
        {
            Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        /// <summary>Switches a numeric value to a relation Tag.</summary>
        public Command<IRelationPart> TagCommand { get; private set; }

        private void OnTagCommandExecute(IRelationPart numericPart)
        {
            var multiplicationDefinition = new MultiplicationRelationDefinitionTag(Model.ParentPage, Model.Origin);

            var multiplicationViewModel = new MultiplicationRelationDefinitionTagViewModel(multiplicationDefinition);
            var multiplicationResult = multiplicationViewModel.ShowWindowAsDialog();
            if (multiplicationResult != true)
            {
                return;
            }

            multiplicationDefinition.Factors.Clear();

            foreach (var relationPart in multiplicationViewModel.Factors)
            {
                multiplicationDefinition.Factors.Add(relationPart);
            }

            var index = Addends.IndexOf(numericPart);
            Addends.Remove(numericPart);
            Addends.Insert(index, multiplicationDefinition);
        }

        /// <summary>Switches a relation tag for a numeric value.</summary>
        public Command<IRelationPart> UntagCommand { get; private set; }

        private void OnUntagCommandExecute(IRelationPart relationPart)
        {
            var index = Addends.IndexOf(relationPart);
            Addends.Remove(relationPart);
            Addends.Insert(index, new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        /// <summary>Calculates the value of the sum</summary>
        public Command CalculateSumCommand { get; private set; }

        private void OnCalculateSumCommandExecute()
        {
            Sum = Addends.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x + y);

            Model.Addends.Clear();
            Model.Addends.AddRange(Addends);
        }

        /// <summary>Validates and confirms changes to the person.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            OnCalculateSumCommandExecute();

            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the person.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CloseViewModelAsync(false);
        }

        #endregion //Commands
    }
}
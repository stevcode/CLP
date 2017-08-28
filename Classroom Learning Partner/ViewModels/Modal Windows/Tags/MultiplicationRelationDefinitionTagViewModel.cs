using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultiplicationRelationDefinitionTagViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public MultiplicationRelationDefinitionTagViewModel(MultiplicationRelationDefinitionTag multiplicationRelationDefinition)
        {
            Model = multiplicationRelationDefinition;
            InitializeCommands();

            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        #endregion //Constructor

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public MultiplicationRelationDefinitionTag Model
        {
            get { return GetValue<MultiplicationRelationDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(MultiplicationRelationDefinitionTag));

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public MultiplicationRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<MultiplicationRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(MultiplicationRelationDefinitionTag.RelationTypes));

        /// <summary>Value of the Product.</summary>
        [ViewModelToModel("Model")]
        public double Product
        {
            get { return GetValue<double>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(double));

        #endregion //Model

        #region Bindings

        /// <summary>List of all the factors in the multiplication relation.</summary>
        public ObservableCollection<IRelationPart> Factors
        {
            get { return GetValue<ObservableCollection<IRelationPart>>(FactorsProperty); }
            set { SetValue(FactorsProperty, value); }
        }

        public static readonly PropertyData FactorsProperty = RegisterProperty("Factors", typeof(ObservableCollection<IRelationPart>), () => new ObservableCollection<IRelationPart>());

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            AddFactorCommand = new Command(OnAddFactorCommandExecute);
            TagCommand = new TaskCommand<IRelationPart>(OnTagCommandExecuteAsync);
            UntagCommand = new Command<IRelationPart>(OnUntagCommandExecute);
            CalculateProductCommand = new Command(OnCalculateProductCommandExecute);
            ConfirmChangesCommand = new TaskCommand(OnConfirmChangesCommandExecuteAsync);
            CancelChangesCommand = new TaskCommand(OnCancelChangesCommandExecuteAsync);
        }

        /// <summary>Adds a zero value factor to the relation definition.</summary>
        public Command AddFactorCommand { get; private set; }

        private void OnAddFactorCommandExecute()
        {
            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        /// <summary>Switches a numeric value to a relation Tag.</summary>
        public TaskCommand<IRelationPart> TagCommand { get; private set; }

        private async Task OnTagCommandExecuteAsync(IRelationPart numericPart)
        {
            var additionDefinition = new AdditionRelationDefinitionTag(Model.ParentPage, Model.Origin);

            var additionViewModel = new AdditionRelationDefinitionTagViewModel(additionDefinition);
            if (!(await additionViewModel.ShowWindowAsDialogAsync() ?? false))
            {
                return;
            }

            additionDefinition.Addends.Clear();

            foreach (var relationPart in additionViewModel.Addends)
            {
                additionDefinition.Addends.Add(relationPart);
            }

            var index = Factors.IndexOf(numericPart);
            Factors.Remove(numericPart);
            Factors.Insert(index, additionDefinition);
        }

        /// <summary>Switches a relation tag for a numeric value.</summary>
        public Command<IRelationPart> UntagCommand { get; private set; }

        private void OnUntagCommandExecute(IRelationPart relationPart)
        {
            var index = Factors.IndexOf(relationPart);
            Factors.Remove(relationPart);
            Factors.Insert(index, new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        /// <summary>Calculates the value of the product</summary>
        public Command CalculateProductCommand { get; private set; }

        private void OnCalculateProductCommandExecute()
        {
            Product = Factors.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x * y);

            Model.Factors.Clear();
            Model.Factors.AddRange(Factors);
        }

        /// <summary>Validates and confirms changes to the person.</summary>
        public TaskCommand ConfirmChangesCommand { get; private set; }

        private async Task OnConfirmChangesCommandExecuteAsync()
        {
            OnCalculateProductCommandExecute();

            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the person.</summary>
        public TaskCommand CancelChangesCommand { get; private set; }

        private async Task OnCancelChangesCommandExecuteAsync()
        {
            await CloseViewModelAsync(false);
        }

        #endregion //Commands
    }
}
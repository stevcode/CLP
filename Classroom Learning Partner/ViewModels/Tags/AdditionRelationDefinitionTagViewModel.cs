using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class AdditionRelationDefinitionTagViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="AdditionRelationDefinitionTagViewModel" /> class.</summary>
        public AdditionRelationDefinitionTagViewModel(AdditionRelationDefinitionTag additionRelationDefinition)
        {
            Model = additionRelationDefinition;
            Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
            Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));

            AddAddendCommand = new Command(OnAddAddendCommandExecute);
            CalculateSumCommand = new Command(OnCalculateSumCommandExecute);
            TagCommand = new Command<IRelationPart>(OnTagCommandExecute);
            UntagCommand = new Command<IRelationPart>(OnUntagCommandExecute);
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "AdditionRelationDefinitionTagVM"; }
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

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof (AdditionRelationDefinitionTag));

        /// <summary>Value of the Sum.</summary>
        [ViewModelToModel("Model")]
        public double Sum
        {
            get { return GetValue<double>(SumProperty); }
            set { SetValue(SumProperty, value); }
        }

        public static readonly PropertyData SumProperty = RegisterProperty("Sum", typeof (double));

        /// <summary>Type of addition relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public AdditionRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<AdditionRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof (AdditionRelationDefinitionTag.RelationTypes));

        #endregion //Model

        #region Bindings

        /// <summary>List of all the addends in the addition relation.</summary>
        public ObservableCollection<IRelationPart> Addends
        {
            get { return GetValue<ObservableCollection<IRelationPart>>(AddendsProperty); }
            set { SetValue(AddendsProperty, value); }
        }

        public static readonly PropertyData AddendsProperty = RegisterProperty("Addends",
                                                                               typeof (ObservableCollection<IRelationPart>),
                                                                               () => new ObservableCollection<IRelationPart>());

        #endregion //Bindings

        #region Commands

        /// <summary>Adds a zero value addend to the relation definition.</summary>
        public Command AddAddendCommand { get; private set; }

        private void OnAddAddendCommandExecute() { Addends.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin)); }

        /// <summary>Calculates the value of the sum</summary>
        public Command CalculateSumCommand { get; private set; }

        private void OnCalculateSumCommandExecute() { Sum = Addends.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x + y); }

        /// <summary>
        /// Switches a numeric value to a relation Tag.
        /// </summary>
        public Command<IRelationPart> TagCommand { get; private set; }

        private void OnTagCommandExecute(IRelationPart numericPart)
        {
            var multiplicationDefinition = new MultiplicationRelationDefinitionTag(Model.ParentPage, Model.Origin);

            var multiplicationViewModel = new MultiplicationRelationDefinitionTagViewModel(multiplicationDefinition);
            var multiplicationView = new MultiplicationRelationDefinitionTagView(multiplicationViewModel);
            multiplicationView.ShowDialog();

            if (multiplicationView.DialogResult != true)
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

        /// <summary>
        /// Switches a relation tag for a numeric value.
        /// </summary>
        public Command<IRelationPart> UntagCommand { get; private set; }

        private void OnUntagCommandExecute(IRelationPart relationPart)
        {
            var index = Addends.IndexOf(relationPart);
            Addends.Remove(relationPart);
            Addends.Insert(index, new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        #endregion //Commands
    }
}
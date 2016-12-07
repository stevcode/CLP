using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class MultiplicationRelationDefinitionTagViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public MultiplicationRelationDefinitionTagViewModel(MultiplicationRelationDefinitionTag multiplicationRelationDefinition)
        {
            Model = multiplicationRelationDefinition;
            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));

            AddFactorCommand = new Command(OnAddFactorCommandExecute);
            CalculateProductCommand = new Command(OnCalculateProductCommandExecute);
            TagCommand = new Command<IRelationPart>(OnTagCommandExecute);
            UntagCommand = new Command<IRelationPart>(OnUntagCommandExecute);
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

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof (MultiplicationRelationDefinitionTag));

        /// <summary>Value of the Product.</summary>
        [ViewModelToModel("Model")]
        public double Product
        {
            get { return GetValue<double>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof (double));

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public MultiplicationRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<MultiplicationRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof (MultiplicationRelationDefinitionTag.RelationTypes)); 

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

        /// <summary>
        /// Adds a zero value factor to the relation definition.
        /// </summary>
        public Command AddFactorCommand { get; private set; }

        private void OnAddFactorCommandExecute()
        {
            Factors.Add(new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        /// <summary>Calculates the value of the product</summary>
        public Command CalculateProductCommand { get; private set; }

        private void OnCalculateProductCommandExecute() { Product = Factors.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x * y); }

        /// <summary>
        /// Switches a numeric value to a relation Tag.
        /// </summary>
        public Command<IRelationPart> TagCommand { get; private set; }

        private void OnTagCommandExecute(IRelationPart numericPart)
        {
            var additionDefinition = new AdditionRelationDefinitionTag(Model.ParentPage, Model.Origin);

            var additionViewModel = new AdditionRelationDefinitionTagViewModel(additionDefinition);
            var additionView = new AdditionRelationDefinitionTagView(additionViewModel);
            additionView.ShowDialog();

            if (additionView.DialogResult != true)
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

        /// <summary>
        /// Switches a relation tag for a numeric value.
        /// </summary>
        public Command<IRelationPart> UntagCommand { get; private set; }

        private void OnUntagCommandExecute(IRelationPart relationPart)
        {
            var index = Factors.IndexOf(relationPart);
            Factors.Remove(relationPart);
            Factors.Insert(index, new NumericValueDefinitionTag(Model.ParentPage, Model.Origin));
        }

        #endregion //Commands
    }
}
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class MultiplicationRelationDefinitionTagViewModel : ViewModelBase
    {
        public class ValueContainer
        {
            public ValueContainer(double value) { ContainedValue = value; }
            public double ContainedValue { get; set; }
        }

        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public MultiplicationRelationDefinitionTagViewModel(MultiplicationRelationDefinitionTag multiplicationRelationDefinition)
        {
            Model = multiplicationRelationDefinition;
            Factors.Add(new ValueContainer(0));
            Factors.Add(new ValueContainer(0));
            Product = 0;

            AddFactorCommand = new Command(OnAddFactorCommandExecute);
            CalculateProductCommand = new Command(OnCalculateProductCommandExecute);
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "MultiplicationRelationDefinitionTagVM"; }
        }

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public MultiplicationRelationDefinitionTag Model
        {
            get { return GetValue<MultiplicationRelationDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof (MultiplicationRelationDefinitionTag));

        /// <summary>List of all the factors in the multiplication relation.</summary>
        public ObservableCollection<ValueContainer> Factors
        {
            get { return GetValue<ObservableCollection<ValueContainer>>(FactorsProperty); }
            set { SetValue(FactorsProperty, value); }
        }

        public static readonly PropertyData FactorsProperty = RegisterProperty("Factors", typeof(ObservableCollection<double>), () => new ObservableCollection<ValueContainer>());

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

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof (MultiplicationRelationDefinitionTag.RelationTypes));

        /// <summary>
        /// Adds a zero value factor to the relation definition.
        /// </summary>
        public Command AddFactorCommand { get; private set; }

        private void OnAddFactorCommandExecute()
        {
            Factors.Add(new ValueContainer(0));
        }

        /// <summary>Calculates the value of the product</summary>
        public Command CalculateProductCommand { get; private set; }

        private void OnCalculateProductCommandExecute() { Product = Factors.Select(x => x.ContainedValue).Aggregate((x, y) => x * y); }
    }
}
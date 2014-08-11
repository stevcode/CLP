using System.Collections.Generic;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

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
        public List<ValueContainer> Factors
        {
            get { return GetValue<List<ValueContainer>>(FactorsProperty); }
            set { SetValue(FactorsProperty, value); }
        }

        public static readonly PropertyData FactorsProperty = RegisterProperty("Factors", typeof (List<double>), () => new List<ValueContainer>());

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
    }
}
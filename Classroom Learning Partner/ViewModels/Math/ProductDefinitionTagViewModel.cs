using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class ProductDefinitionTagViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDefinitionTagViewModel" /> class.
        /// </summary>
        public ProductDefinitionTagViewModel(ProductDefinitionTag productDefinition) { Model = productDefinition; }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "ProductDefinitionVM"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public ProductDefinitionTag Model
        {
            get { return GetValue<ProductDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(ProductDefinitionTag));

        /// <summary>
        /// Value of the First Factor.
        /// </summary>
        [ViewModelToModel("Model")]
        public double? FirstFactor
        {
            get { return GetValue<double?>(FirstFactorProperty); }
            set { SetValue(FirstFactorProperty, value); }
        }

        public static readonly PropertyData FirstFactorProperty = RegisterProperty("FirstFactor", typeof(double?));

        /// <summary>
        /// Value of the Second Factor.
        /// </summary>
        [ViewModelToModel("Model")]
        public double? SecondFactor
        {
            get { return GetValue<double?>(SecondFactorProperty); }
            set { SetValue(SecondFactorProperty, value); }
        }

        public static readonly PropertyData SecondFactorProperty = RegisterProperty("SecondFactor", typeof(double?));

        /// <summary>
        /// Value of the Product.
        /// </summary>
        [ViewModelToModel("Model")]
        public double? Product
        {
            get { return GetValue<double?>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(double?));

        /// <summary>
        /// Type of Product.
        /// </summary>
        [ViewModelToModel("Model")]
        public ProductType ProductType
        {
            get { return GetValue<ProductType>(ProductTypeProperty); }
            set { SetValue(ProductTypeProperty, value); }
        }

        public static readonly PropertyData ProductTypeProperty = RegisterProperty("ProductType", typeof(ProductType), ProductType.Generic);

        /// <summary>
        /// Part of the Product Definition that is meant to be solved.
        /// </summary>
        [ViewModelToModel("Model")]
        public ProductPart UngivenProductPart
        {
            get { return GetValue<ProductPart>(UngivenProductPartProperty); }
            set { SetValue(UngivenProductPartProperty, value); }
        }

        public static readonly PropertyData UngivenProductPartProperty = RegisterProperty("UngivenProductPart", typeof(ProductPart), ProductPart.Product);
    }
}
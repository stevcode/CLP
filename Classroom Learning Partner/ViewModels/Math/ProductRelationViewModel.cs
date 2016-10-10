using System;
using System.Collections.Generic;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class ProductRelationViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRelationViewModel" /> class.
        /// </summary>
        public ProductRelationViewModel(ProductRelation relation) { Model = relation; }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "ProductRelation view model"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public ProductRelation Model
        {
            get { return GetValue<ProductRelation>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(ProductRelation));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String Factor1
        {
            get { return GetValue<String>(Factor1Property); }
            set { SetValue(Factor1Property, value); }
        }

        public static readonly PropertyData Factor1Property = RegisterProperty("Factor1", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String Factor2
        {
            get { return GetValue<String>(Factor2Property); }
            set { SetValue(Factor2Property, value); }
        }

        public static readonly PropertyData Factor2Property = RegisterProperty("Factor2", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String Product
        {
            get { return GetValue<String>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String Factor1Label
        {
            get { return GetValue<String>(Factor1LabelProperty); }
            set { SetValue(Factor1LabelProperty, value); }
        }

        public static readonly PropertyData Factor1LabelProperty = RegisterProperty("Factor1Label", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String Factor2Label
        {
            get { return GetValue<String>(Factor2LabelProperty); }
            set { SetValue(Factor2LabelProperty, value); }
        }

        public static readonly PropertyData Factor2LabelProperty = RegisterProperty("Factor2Label", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public String ProductLabel
        {
            get { return GetValue<String>(ProductLabelProperty); }
            set { SetValue(ProductLabelProperty, value); }
        }

        public static readonly PropertyData ProductLabelProperty = RegisterProperty("ProductLabel", typeof(String));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public Boolean Factor1Given
        {
            get { return GetValue<Boolean>(Factor1GivenProperty); }
            set { SetValue(Factor1GivenProperty, value); }
        }

        public static readonly PropertyData Factor1GivenProperty = RegisterProperty("Factor1Given", typeof(Boolean));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public Boolean Factor2Given
        {
            get { return GetValue<Boolean>(Factor2GivenProperty); }
            set { SetValue(Factor2GivenProperty, value); }
        }

        public static readonly PropertyData Factor2GivenProperty = RegisterProperty("Factor2Given", typeof(Boolean));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public Boolean ProductGiven
        {
            get { return GetValue<Boolean>(ProductGivenProperty); }
            set { SetValue(ProductGivenProperty, value); }
        }

        public static readonly PropertyData ProductGivenProperty = RegisterProperty("ProductGiven", typeof(Boolean));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Model")]
        public ProductRelation.ProductRelationTypes RelationType
        {
            get { return GetValue<ProductRelation.ProductRelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(ProductRelation.ProductRelationTypes));

        public IEnumerable<ProductRelation.ProductRelationTypes> ProductRelationTypeValues
        {
            get { return (IEnumerable<ProductRelation.ProductRelationTypes>)Enum.GetValues(typeof(ProductRelation.ProductRelationTypes)); }
        }
    }
}
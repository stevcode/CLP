using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    /// <summary>
    /// ProductRelation Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class ProductRelation : MathRelation
    {
        public enum ProductRelationTypes
        {
            GenericProduct,
            EqualGroups,
            Area
        };

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public ProductRelation() { }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected ProductRelation(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ProductRelationTypes RelationType
        {
            get { return GetValue<ProductRelationTypes>(RelationTypeProperty); }
            set
            {
                SetValue(RelationTypeProperty, value);
                // Changing this value also changes the various labels
                switch(value)
                {
                    case ProductRelationTypes.GenericProduct:
                        TypeLabel = "Generic Product";
                        Factor1Label = "First Factor";
                        Factor2Label = "Second Factor";
                        ProductLabel = "Product";
                        break;
                    case ProductRelationTypes.EqualGroups:
                        TypeLabel = "Equal Groups";
                        Factor1Label = "Number of Groups";
                        Factor2Label = "Items Per Group";
                        ProductLabel = "Total Items";
                        break;
                    case ProductRelationTypes.Area:
                        TypeLabel = "Array";
                        Factor1Label = "Width";
                        Factor2Label = "Height";
                        ProductLabel = "Area";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Register the RelationType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(ProductRelationTypes), ProductRelationTypes.GenericProduct);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String TypeLabel
        {
            get { return GetValue<String>(TypeLabelProperty); }
            set { SetValue(TypeLabelProperty, value); }
        }

        /// <summary>
        /// Register the TypeLabel property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TypeLabelProperty = RegisterProperty("TypeLabel", typeof(String), "Generic Product");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String Factor1
        {
            get { return GetValue<String>(Factor1Property); }
            set { SetValue(Factor1Property, value); }
        }

        /// <summary>
        /// Register the Factor1 property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor1Property = RegisterProperty("Factor1", typeof(String), "2");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String Factor2
        {
            get { return GetValue<String>(Factor2Property); }
            set { SetValue(Factor2Property, value); }
        }

        /// <summary>
        /// Register the Factor2 property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor2Property = RegisterProperty("Factor2", typeof(String), "3");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String Product
        {
            get { return GetValue<String>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        /// <summary>
        /// Register the name property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(String), "6");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Boolean Factor1Given
        {
            get { return GetValue<Boolean>(Factor1GivenProperty); }
            set { SetValue(Factor1GivenProperty, value); }
        }

        /// <summary>
        /// Register the Factor1Given property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor1GivenProperty = RegisterProperty("Factor1Given", typeof(Boolean), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Boolean Factor2Given
        {
            get { return GetValue<Boolean>(Factor2GivenProperty); }
            set { SetValue(Factor2GivenProperty, value); }
        }

        /// <summary>
        /// Register the Factor2Given property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor2GivenProperty = RegisterProperty("Factor2Given", typeof(Boolean), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Boolean ProductGiven
        {
            get { return GetValue<Boolean>(ProductGivenProperty); }
            set { SetValue(ProductGivenProperty, value); }
        }

        /// <summary>
        /// Register the ProductGiven property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ProductGivenProperty = RegisterProperty("ProductGiven", typeof(Boolean), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String Factor1Label
        {
            get { return GetValue<String>(Factor1LabelProperty); }
            set { SetValue(Factor1LabelProperty, value); }
        }

        /// <summary>
        /// Register the Factor1Label property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor1LabelProperty = RegisterProperty("Factor1Label", typeof(String), "First Factor");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String Factor2Label
        {
            get { return GetValue<String>(Factor2LabelProperty); }
            set { SetValue(Factor2LabelProperty, value); }
        }

        /// <summary>
        /// Register the Factor2Label property so it is known in the class.
        /// </summary>
        public static readonly PropertyData Factor2LabelProperty = RegisterProperty("Factor2Label", typeof(String), "Second Factor");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public String ProductLabel
        {
            get { return GetValue<String>(ProductLabelProperty); }
            set { SetValue(ProductLabelProperty, value); }
        }

        /// <summary>
        /// Register the ProductLabel property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ProductLabelProperty = RegisterProperty("ProductLabel", typeof(String), "Product");

        #endregion

        #region Methods

        public override String GetExampleNumberSentence()
        {
            var factor1String = (Factor1Given ? Factor1 : "[" + Factor1 + "]");
            var factor2String = (Factor2Given ? Factor2 : "[" + Factor2 + "]");
            var productString = (ProductGiven ? Product : "[" + Product + "]");
            return factor1String + " * " + factor2String + " = " + productString;
        }

        public static String toString(ProductRelation p)
        {
            return "factor1:" + p.Factor1 + "," + p.Factor1Given + "," + p.Factor1Label + "\n" + "factor2:" + p.Factor2 + "," + p.Factor2Given + "," + p.Factor2Label + "\n" + "product:" + p.Product +
                   "," + p.ProductGiven + "," + p.ProductLabel + "\n" + p.TypeLabel;
        }

        public static ProductRelation fromString(String source)
        {
            var p = new ProductRelation();
            var lines = source.Split('\n');
            if(lines[0].StartsWith("factor1:"))
            {
                var factor1info = lines[0].Substring(8).Split(',');
                p.Factor1 = factor1info[0];
                p.Factor1Given = Boolean.Parse(factor1info[1]);
                p.Factor1Label = factor1info[2];
            }
            if(lines[1].StartsWith("factor2:"))
            {
                var factor2info = lines[1].Substring(8).Split(',');
                p.Factor2 = factor2info[0];
                p.Factor2Given = Boolean.Parse(factor2info[1]);
                p.Factor2Label = factor2info[2];
            }
            if(lines[2].StartsWith("product:"))
            {
                var productinfo = lines[2].Substring(8).Split(',');
                p.Product = productinfo[0];
                p.ProductGiven = Boolean.Parse(productinfo[1]);
                p.ProductLabel = productinfo[2];
            }
            switch(lines[3])
            {
                case "Equal Groups":
                    p.RelationType = ProductRelationTypes.EqualGroups;
                    break;
                case "Array":
                    p.RelationType = ProductRelationTypes.Area;
                    break;
                case "Generic Product":
                default:
                    p.RelationType = ProductRelationTypes.GenericProduct;
                    break;
            }
            return p;
        }

        #endregion
    }
}
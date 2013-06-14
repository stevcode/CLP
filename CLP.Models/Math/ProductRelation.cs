using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using System.Runtime.Serialization;

namespace CLP.Models
{

    /// <summary>
    /// ProductRelation Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ProductRelation : MathRelation
    {
        #region Fields
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public ProductRelation() { }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ProductRelation(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif
        #endregion

        #region Properties
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
            String factor1String = (Factor1Given ? Factor1 : "[" + Factor1 + "]");
            String factor2String = (Factor2Given ? Factor2 : "[" + Factor2 + "]");
            String productString = (ProductGiven ? Product : "[" + Product + "]");
            return factor1String + " * " + factor2String + " = " + productString;
        }

        #endregion
    }

}

using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum ProductType
    {
        Generic,
        EqualGroups,
        Area
    }

    public enum ProductPart
    {
        FirstFactor,
        SecondFactor,
        Product
    }

    [Serializable]
    public class ProductDefinitionTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ProductDefinitionTag" /> from scratch.
        /// </summary>
        public ProductDefinitionTag() { }

        /// <summary>
        /// Initializes <see cref="ProductDefinitionTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ProductDefinitionTag" /> belongs to.</param>
        public ProductDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="ProductDefinitionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ProductDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the First Factor.
        /// </summary>
        public double? FirstFactor
        {
            get { return GetValue<double?>(FirstFactorProperty); }
            set { SetValue(FirstFactorProperty, value); }
        }

        public static readonly PropertyData FirstFactorProperty = RegisterProperty("FirstFactor", typeof(double?));

        /// <summary>
        /// Value of the Second Factor.
        /// </summary>
        public double? SecondFactor
        {
            get { return GetValue<double?>(SecondFactorProperty); }
            set { SetValue(SecondFactorProperty, value); }
        }

        public static readonly PropertyData SecondFactorProperty = RegisterProperty("SecondFactor", typeof(double?));

        /// <summary>
        /// Value of the Product.
        /// </summary>
        public double? Product
        {
            get { return GetValue<double?>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(double?));

        /// <summary>
        /// Type of Product.
        /// </summary>
        public ProductType ProductType
        {
            get { return GetValue<ProductType>(ProductTypeProperty); }
            set { SetValue(ProductTypeProperty, value); }
        }

        public static readonly PropertyData ProductTypeProperty = RegisterProperty("ProductType", typeof(ProductType), ProductType.Generic);

        /// <summary>
        /// Part of the Product Definition that is meant to be solved.
        /// </summary>
        public ProductPart UngivenProductPart
        {
            get { return GetValue<ProductPart>(UngivenProductPartProperty); }
            set { SetValue(UngivenProductPartProperty, value); }
        }

        public static readonly PropertyData UngivenProductPartProperty = RegisterProperty("UngivenProductPart", typeof(ProductPart), ProductPart.Product);

        #region Calculated Properties

        public string FirstFactorLabel
        {
            get
            {
                switch(ProductType)
                {
                    case ProductType.Generic:
                        return "First Factor";
                    case ProductType.EqualGroups:
                        return "Number of Groups";
                    case ProductType.Area:
                        return "Width";
                    default:
                        return "ERROR";
                }
            }
        }

        public string SecondFactorLabel
        {
            get
            {
                switch(ProductType)
                {
                    case ProductType.Generic:
                        return "Second Factor";
                    case ProductType.EqualGroups:
                        return "Items per Group";
                    case ProductType.Area:
                        return "Height";
                    default:
                        return "ERROR";
                }
            }
        }

        public string ProductLabel
        {
            get
            {
                switch(ProductType)
                {
                    case ProductType.Generic:
                        return "Product";
                    case ProductType.EqualGroups:
                        return "Total Items";
                    case ProductType.Area:
                        return "Area";
                    default:
                        return "ERROR";
                }
            }
        }

        #endregion //Calculated Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Product Type: {0}\n" + "{1}: {2}\n" + "{3}: {4}\n" + "{5}: {6}\n" + "Ungiven Product Part: {7}", 
                                       ProductType,
                                       FirstFactorLabel,
                                       FirstFactor,
                                       SecondFactorLabel,
                                       SecondFactor,
                                       ProductLabel,
                                       Product,
                                       UngivenProductPart); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
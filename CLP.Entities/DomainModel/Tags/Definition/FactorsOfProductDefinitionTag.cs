using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class FactorsOfProductDefinitionTag : ATagBase
    {
        public enum FactorAmounts
        {
            AllPairs,
            ExactNumberOfPairs,
            MinimumNumberOfPairs,
            RequireReversePairs,
            All,
            ExactAmount,
            MinimumAmount
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FactorsOfProductDefinitionTag" /> from scratch.
        /// </summary>
        public FactorsOfProductDefinitionTag() { }

        /// <summary>
        /// Initializes <see cref="FactorsOfProductDefinitionTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FactorsOfProductDefinitionTag" /> belongs to.</param>
        public FactorsOfProductDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="FactorsOfProductDefinitionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FactorsOfProductDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Final product to generate factors from.
        /// </summary>
        public uint Product
        {
            get { return GetValue<uint>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof (uint), 0);

        /// <summary>
        /// A list of all the factors of the Product.
        /// </summary>
        public List<uint> AllFactors
        {
            get { return GetValue<List<uint>>(AllFactorsProperty); }
            set { SetValue(AllFactorsProperty, value); }
        }

        public static readonly PropertyData AllFactorsProperty = RegisterProperty("AllFactors", typeof (List<uint>), () => new List<uint>());

        public List<string> AllFactorPairs
        {
            get { return AllFactors.Where(f => f != Product).Select(f => string.Format("{0}x{1}", f, Product / f)).ToList(); }
        }

        /// <summary>
        /// Required number of factors to be listed to satisfy definition.
        /// </summary>
        public FactorAmounts RequiredAmountOfFactors
        {
            get { return GetValue<FactorAmounts>(RequiredAmountOfFactorsProperty); }
            set { SetValue(RequiredAmountOfFactorsProperty, value); }
        }

        public static readonly PropertyData RequiredAmountOfFactorsProperty = RegisterProperty("RequiredAmountOfFactors", typeof(FactorAmounts), FactorAmounts.AllPairs);

        /// <summary>
        /// If Exact or Minimum number of Factors are required, limit by this value.
        /// </summary>
        public uint NumberOfRequiredFactors
        {
            get { return GetValue<uint>(NumberOfRequiredFactorsProperty); }
            set { SetValue(NumberOfRequiredFactorsProperty, value); }
        }

        public static readonly PropertyData NumberOfRequiredFactorsProperty = RegisterProperty("NumberOfRequiredFactors", typeof (uint), 0);

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedName
        {
            get
            {
                switch (RequiredAmountOfFactors)
                {
                    case FactorAmounts.AllPairs:
                    case FactorAmounts.ExactNumberOfPairs:
                    case FactorAmounts.MinimumNumberOfPairs:
                    case FactorAmounts.RequireReversePairs:
                        return "Factor Pairs Of A Product Definition";
                    case FactorAmounts.All:
                    case FactorAmounts.ExactAmount:
                    case FactorAmounts.MinimumAmount:
                        return "Factors Of A Product Definition";
                    default:
                        return "Factors Of A Product Definition";
                }
            }
        }

        public override string FormattedValue
        {
            get
            {
                var singleOrPairs = string.Empty;
                switch (RequiredAmountOfFactors)
                {
                    case FactorAmounts.AllPairs:
                    case FactorAmounts.ExactNumberOfPairs:
                    case FactorAmounts.MinimumNumberOfPairs:
                    case FactorAmounts.RequireReversePairs:
                        singleOrPairs = "Factor Pairs";
                        break;
                    case FactorAmounts.All:
                    case FactorAmounts.ExactAmount:
                    case FactorAmounts.MinimumAmount:
                        singleOrPairs = "Factors";
                        break;
                }

                return string.Format("{0} has the following {1}:\n", Product, singleOrPairs);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Methods

        private void CalculateAllFactors()
        {
            AllFactors.Clear();
            if (Product == 0)
            {
                return;
            }

            var halfOfProduct = Product / 2;

            for (var i = 1; i <= halfOfProduct; i++)
            {
                if (Product % i == 0)
                {
                    AllFactors.Add((uint)i);
                }
            }

            AllFactors.Add(Product);

            AllFactors = AllFactors.Distinct().ToList();
        }

        #endregion //Methods
    }
}
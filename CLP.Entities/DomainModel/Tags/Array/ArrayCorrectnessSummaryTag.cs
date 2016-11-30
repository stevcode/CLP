using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Ann
{
    public enum ArrayIncorrectReasons
    {
        Representation,
        XAxisDivider,
        YAxisDivider
    }

    [Serializable]
    public class ArrayCorrectnessSummaryTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ArrayCorrectnessSummaryTag" /> from scratch.</summary>
        public ArrayCorrectnessSummaryTag() { }

        /// <summary>Initializes <see cref="ArrayCorrectnessSummaryTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayCorrectnessSummaryTag" /> belongs to.</param>
        public ArrayCorrectnessSummaryTag(CLPPage parentPage, Origin origin, Correctness correctness, List<ArrayIncorrectReasons> incorrectReasons)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            Correctness = correctness;
            ArrayIncorrectReasons = incorrectReasons;
        }

        /// <summary>Initializes <see cref="ArrayCorrectnessSummaryTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayCorrectnessSummaryTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof (Correctness));

        /// <summary>Reason the Interpreted Correctness was set to Incorrect.</summary>
        public List<ArrayIncorrectReasons> ArrayIncorrectReasons
        {
            get { return GetValue<List<ArrayIncorrectReasons>>(ArrayIncorrectReasonsProperty); }
            set { SetValue(ArrayIncorrectReasonsProperty, value); }
        }

        public static readonly PropertyData ArrayIncorrectReasonsProperty = RegisterProperty("ArrayIncorrectReasons",
                                                                                             typeof (List<ArrayIncorrectReasons>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Correctness Summary"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                                         ? string.Empty
                                         : " due to: " + string.Join(", ", ArrayIncorrectReasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
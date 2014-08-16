using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionTemplateIncorrectReason
    {
        WrongDividend,
        WrongDivisor,
        WrongDividendAndDivisor,
        Incomplete,
        Other
    }

    [Serializable]
    public class DivisionTemplateRepresentationCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateRepresentationCorrectnessTag" /> from scratch.</summary>
        public DivisionTemplateRepresentationCorrectnessTag() { }

        /// <summary>Initializes <see cref="DivisionTemplateRepresentationCorrectnessTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateRepresentationCorrectnessTag" /> belongs to.</param>
        public DivisionTemplateRepresentationCorrectnessTag(CLPPage parentPage,
                                                            Origin origin,
                                                            Correctness correctness,
                                                            List<DivisionTemplateIncorrectReason> incorrectReasons)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            DivisionTemplateIncorrectReasons = incorrectReasons;
        }

        /// <summary>Initializes <see cref="ArrayRepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
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

        /// <summary>Reasons the Interpreted Correctness was set to Incorrect.</summary>
        public List<DivisionTemplateIncorrectReason> DivisionTemplateIncorrectReasons
        {
            get { return GetValue<List<DivisionTemplateIncorrectReason>>(DivisionTemplateIncorrectReasonsProperty); }
            set { SetValue(DivisionTemplateIncorrectReasonsProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIncorrectReasonsProperty = RegisterProperty("DivisionTemplateIncorrectReasons",
                                                                                                        typeof (List<DivisionTemplateIncorrectReason>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                                         ? string.Empty
                                         : " due to:\n" + string.Join("\n", DivisionTemplateIncorrectReasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
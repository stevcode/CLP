using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionToolIncorrectReason
    {
        WrongDividend,
        WrongDivisor,
        WrongDividendAndDivisor,
        Incomplete,
        Other
    }

    [Serializable]
    public class DivisionToolRepresentationCorrectnessTag : ADivisionToolBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolRepresentationCorrectnessTag" /> from scratch.</summary>
        public DivisionToolRepresentationCorrectnessTag() { }

        /// <summary>Initializes <see cref="DivisionToolRepresentationCorrectnessTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolRepresentationCorrectnessTag" /> belongs to.</param>
        public DivisionToolRepresentationCorrectnessTag(CLPPage parentPage,
                                                            Origin origin,
                                                            string divisionToolID,
                                                            double dividend,
                                                            double divisor,
                                                            int divisionToolNumber,
                                                            Correctness correctness,
                                                            List<DivisionToolIncorrectReason> incorrectReasons)
            : base(parentPage, origin, divisionToolID, dividend, divisor, divisionToolNumber)
        {
            Correctness = correctness;
            DivisionToolIncorrectReasons = incorrectReasons;
        }

        /// <summary>Initializes <see cref="ArrayRepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
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
        public List<DivisionToolIncorrectReason> DivisionToolIncorrectReasons
        {
            get { return GetValue<List<DivisionToolIncorrectReason>>(DivisionToolIncorrectReasonsProperty); }
            set { SetValue(DivisionToolIncorrectReasonsProperty, value); }
        }

        public static readonly PropertyData DivisionToolIncorrectReasonsProperty = RegisterProperty("DivisionToolIncorrectReasons",
                                                                                                        typeof (List<DivisionToolIncorrectReason>));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Division Template {0} Representation Correctness", DivisionToolNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Correctness for {0} / {1}\n" + "DivisionTool {2} on page.\n" + "{3}{4}",
                                     Dividend,
                                     Divisor,
                                     IsDivisionToolStillOnPage ? "still" : "no longer",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                                         ? string.Empty
                                         : " due to:\n" + string.Join("\n", DivisionToolIncorrectReasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
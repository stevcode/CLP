using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum NumberLineRepresentationIncorrectReasons
    {
        Incomplete,
        WrongNumberofJumps,
        WrongJumpSizes,
        ReversedGrouping,
        WrongLastMarkedTick
    }

    [Serializable]
    public class NumberLineRepresentationCorrectnessTag : ANumberLineBaseTag
    {
        #region Constructors

        public NumberLineRepresentationCorrectnessTag() { }

        public NumberLineRepresentationCorrectnessTag(CLPPage parentPage,
                                          Origin origin,
                                          string numberLineID,
                                          int firstNumber,
                                          int lastNumber,
                                          int numberLineNumber,
                                          Correctness correctness,
                                          List<NumberLineRepresentationIncorrectReasons> reasons)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            Correctness = correctness;
            Reasons = reasons;
        }

        public NumberLineRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness));


        /// <summary>
        /// Reason Number Line is wrong
        /// </summary>
        public List<NumberLineRepresentationIncorrectReasons> Reasons
        {
            get { return GetValue<List<NumberLineRepresentationIncorrectReasons>>(ReasonsProperty); }
            set { SetValue(ReasonsProperty, value); }
        }

        public static readonly PropertyData ReasonsProperty = RegisterProperty("Reasons", typeof (List<NumberLineRepresentationIncorrectReasons>));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Correctness for Number Line {0}", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Correctness for {0} to {1}.\n" + "Number Line {2} on page.\n" + "{3}.\n" + "{4}",
                                     FirstNumber,
                                     LastNumber,
                                     IsNumberLineStillOnPage ? "still" : "no longer",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                                         ? string.Empty
                                         : " due to:\n" + string.Join("\n", Reasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
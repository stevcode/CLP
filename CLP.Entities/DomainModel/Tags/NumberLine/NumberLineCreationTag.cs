using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineCreationTag: ANumberLineBaseTag
    {
        #region Constructors

        public NumberLineCreationTag() { }

        public NumberLineCreationTag(CLPPage parentPage, Origin origin, string numberLineID, int firstNumber, int lastNumber, int numberLineNumber, double distanceFromAnswer)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            DistanceFromAnswer = distanceFromAnswer;
        }

        public NumberLineCreationTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double DistanceFromAnswer
        {
            get { return GetValue<double>(DistanceFromAnswerProperty); }
            set { SetValue(DistanceFromAnswerProperty, value); }
        }

        public static readonly PropertyData DistanceFromAnswerProperty = RegisterProperty("DistanceFromAnswer", typeof(double));


        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Number Line {0} Created", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Number Line from {0}  to {1} created.\n" + "Number Line {2} on page.\n" + "Number line is {3} by {4}.",
                                     FirstNumber,
                                     LastNumber,
                                     IsNumberLineStillOnPage ? "still" : "no longer",
                                     DistanceFromAnswer < 0 ? "too short" : "too long",
                                     Math.Abs(DistanceFromAnswer));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
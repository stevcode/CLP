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

        public NumberLineCreationTag(CLPPage parentPage, Origin origin, string numberLineID, int firstNumber, int lastNumber, int numberLineNumber, bool isSmaller, int farFromAnswer)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            FarFromAnswer = farFromAnswer;
            IsSmaller = isSmaller;
        }

        public NumberLineCreationTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public int FarFromAnswer
        {
            get { return GetValue<int>(FarFromAnswerProperty); }
            set { SetValue(FarFromAnswerProperty, value); }
        }

        public static readonly PropertyData FarFromAnswerProperty = RegisterProperty("FarFromAnswer", typeof(int));


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
                                     IsSmaller ? "smaller, larger"
                                     0);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
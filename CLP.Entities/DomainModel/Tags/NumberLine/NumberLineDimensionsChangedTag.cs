using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineDimensionsChangedTag : ANumberLineBaseTag
    {
        #region Constructors

        public NumberLineDimensionsChangedTag() { }

        public NumberLineDimensionsChangedTag(CLPPage parentPage,
                                          Origin origin,
                                          string numberLineID,
                                          int firstNumber,
                                          int lastNumber,
                                          int numberLineNumber,
                                          int newFirstNumber,
                                          int newLastNumber,
                                          int changeSize,
                                          bool isClicked,
                                          double oldDistanceFromAnswer,
                                          double newDistanceFromAnswer,
                                          int? tooLowNumber)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            NewFirstNumber = newFirstNumber;
            NewLastNumber = newLastNumber;
            ChangeSize = changeSize;
            ArrowClicked = isClicked;
            OldDistanceFromAnswer = oldDistanceFromAnswer;
            NewDistanceFromAnswer = newDistanceFromAnswer;
            TooLowNumber = tooLowNumber;
        }

        public NumberLineDimensionsChangedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// First Number after Resizing
        /// </summary>
        public int NewFirstNumber
        {
            get { return GetValue<int>(NewFirstNumberProperty); }
            set { SetValue(NewFirstNumberProperty, value); }
        }

        public static readonly PropertyData NewFirstNumberProperty = RegisterProperty("NewFirstNumber", typeof (int));

        /// <summary>
        /// Last Number after Resizing
        /// </summary>
        public int NewLastNumber
        {
            get { return GetValue<int>(NewLastNumberProperty); }
            set { SetValue(NewLastNumberProperty, value); }
        }

        public static readonly PropertyData NewLastNumberProperty = RegisterProperty("NewLastNumber", typeof (int));

        /// <summary>
        /// How much Number Line was resized by
        /// </summary>
        public int ChangeSize
        {
            get { return GetValue<int>(ChangeSizeProperty); }
            set { SetValue(ChangeSizeProperty, value); }
        }

        public static readonly PropertyData ChangeSizeProperty = RegisterProperty("ChangeSize", typeof (int), 0);

        /// <summary>
        /// Was arrow clicked or dragged
        /// </summary>
        public bool ArrowClicked
        {
            get { return GetValue<bool>(ArrowClickedProperty); }
            set { SetValue(ArrowClickedProperty, value); }
        }

        public static readonly PropertyData ArrowClickedProperty = RegisterProperty("ArrowClicked", typeof (bool), true);

        /// <summary>
        /// Old Last Tick distance from answer
        /// </summary>
        public double OldDistanceFromAnswer
        {
            get { return GetValue<double>(OldDistanceFromAnswerProperty); }
            set { SetValue(OldDistanceFromAnswerProperty, value); }
        }

        public static readonly PropertyData OldDistanceFromAnswerProperty = RegisterProperty("OldDistanceFromAnswer", typeof (double));

        /// <summary>
        /// New Last Tick distance from answer
        /// </summary>
        public double NewDistanceFromAnswer
        {
            get { return GetValue<double>(NewDistanceFromAnswerProperty); }
            set { SetValue(NewDistanceFromAnswerProperty, value); }
        }

        public static readonly PropertyData NewDistanceFromAnswerProperty = RegisterProperty("NewDistanceFromAnswer", typeof (double));

        /// <summary>
        /// Too low number is number line is too low
        /// </summary>
        public int? TooLowNumber
        {
            get { return GetValue<int?>(TooLowNumberProperty); }
            set { SetValue(TooLowNumberProperty, value); }
        }

        public static readonly PropertyData TooLowNumberProperty = RegisterProperty("TooLowNumber", typeof (int?));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Number Line {0} Resized", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format(
                                  "Number Line from {0}  to {1} is now from {2} to {3}.\n" + "Number Line {4} on page.\n" + "Number Line {5} by {6}.\n" +
                                  "Arrow was {7} to set new size.\n" + "{8}" + "Was {9} from answer and now is {10} from answer.",
                                  FirstNumber,
                                  LastNumber,
                                  NewFirstNumber,
                                  NewLastNumber,
                                  IsNumberLineStillOnPage ? "still" : "no longer",
                                  ChangeSize < 0 ? "shrunk" : "grew",
                                  Math.Abs(ChangeSize),
                                  ArrowClicked ? "clicked" : "dragged",
                                  TooLowNumber == null ? String.Empty : "Tried to set End Number to " + TooLowNumber + ".\n",
                                  Math.Abs(OldDistanceFromAnswer),
                                  Math.Abs(NewDistanceFromAnswer));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
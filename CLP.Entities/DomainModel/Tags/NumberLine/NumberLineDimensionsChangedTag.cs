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
                                          List<int> jumpSizes,
                                          int? lastMarkedNumber
                                          )
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            JumpSizes = jumpSizes;
            LastMarkedNumber = lastMarkedNumber;
        }

        public NumberLineDimensionsChangedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Number Line {0} Deleted", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Number Line from {0}  to {1} is now from {2} to {3}.\n" + "Number Line {4} on page.\n" + "Number Line {5} by {6}.\n" + "Arrow was {7} to set new size.\n" + "Was {8} from answer and now is {9} from answer.",
                                     OldFirstNumber,
                                     OldLastNumber,
                                     NewFirstNumber,
                                     NewLastNumber,
                                     IsNumberLineStillOnPage ? "still" : "no longer",
                                     ChangeSize < 0 ? "shrunk" : "grew",
                                     ChangeSize,
                                     ArrowClicked ? 
                                     );
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
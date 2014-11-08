using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineDeletedTag : ADivisionTemplateBaseTag
    {
        #region Constructors

        public NumberLineDeletedTag() { }

        public NumberLineDeletedTag(CLPPage parentPage,
                                          Origin origin,
                                          string numberLineID,
                                          int firstNumber,
                                          int lastNumber,
                                          int numberLineNumber,
                                          List<string> jumpSizes,
                                          int lastMarkedNumber
                                          )
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            JumpSizes = jumpSizes;
            LastMarkedNumber = lastMarkedNumber;
        }

        public NumberLineDeletedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public List<string> JumpSizes
        {
            get { return GetValue<List<string>>(JumpSizesProperty); }
            set { SetValue(JumpSizesProperty, value); }
        }

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes", typeof(List<string>));

        public int LastMarkedNumber
        {
            get { return GetValue<List<string>>(LastMarkedNumberProperty); }
            set { SetValue(LastMarkedNumberProperty, value); }
        }

        public static readonly PropertyData LastMarkedNumberProperty = RegisterProperty("LastMarkedNumber", typeof (int));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Number Line {0} Deleted", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Number Line from {0}  to {1} deleted.\n" + "Number Line {2} on page.\n" + "Jump Sizes: {3}.\n" + "Last marked number: {4}.",
                                     FirstNumber,
                                     LastNumber,
                                     IsNumberLineStillOnPage ? "still" : "no longer",
                                     string.Join(",", JumpSizes),
                                     LastMarkedNumber);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
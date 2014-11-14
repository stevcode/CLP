using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineCompletenessTag : ANumberLineBaseTag
    {
        #region Constructors

        public NumberLineCompletenessTag() { }

        public NumberLineCompletenessTag(CLPPage parentPage,
                                         Origin origin,
                                         string numberLineID,
                                         int firstNumber,
                                         int lastNumber,
                                         int numberLineNumber,
                                         bool numberLineComplete,
                                         bool noJumps,
                                         int gaps,
                                         int overlaps)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            NumberLineComplete = numberLineComplete;
            NoJumps = noJumps;
            Gaps = gaps;
            Overlaps = overlaps;
        }

        public NumberLineCompletenessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Is the Number Line Complete
        /// </summary>
        public bool NumberLineComplete
        {
            get { return GetValue<bool>(NumberLineCompleteProperty); }
            set { SetValue(NumberLineCompleteProperty, value); }
        }

        public static readonly PropertyData NumberLineCompleteProperty = RegisterProperty("NumberLineComplete", typeof (bool));

        /// <summary>
        /// Was it a numnber line without any jumps
        /// </summary>
        public bool NoJumps
        {
            get { return GetValue<bool>(NoJumpsProperty); }
            set { SetValue(NoJumpsProperty, value); }
        }

        public static readonly PropertyData NoJumpsProperty = RegisterProperty("NoJumps", typeof (bool));

        /// <summary>
        /// If gaps exist, how many gaps are there
        /// </summary>
        public int Gaps
        {
            get { return GetValue<int>(GapsProperty); }
            set { SetValue(GapsProperty, value); }
        }

        public static readonly PropertyData GapsProperty = RegisterProperty("Gaps", typeof (int));

        /// <summary>
        /// If overlaps exist, how many overlaps are there
        /// </summary>
        public int Overlaps
        {
            get { return GetValue<int>(OverlapsProperty); }
            set { SetValue(OverlapsProperty, value); }
        }

        public static readonly PropertyData OverlapsProperty = RegisterProperty("Overlaps", typeof (int));

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Completeness for Number Line {0}", NumberLineNumber); }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Completeness for {0}  to {1}.\n" + "Number Line {2} on page.\n" + "Number Line is {3}." + "{4}" + "{5}" + "{6}",
                                     FirstNumber,
                                     LastNumber,
                                     IsNumberLineStillOnPage ? "still" : "no longer",
                                     NumberLineComplete ? "Complete" : "Incomplete",
                                     NumberLineComplete ? string.Empty : (NoJumps ? "\nNo Jumps." : string.Empty),
                                     NumberLineComplete ? string.Empty : (Gaps == 0 ? string.Empty : "\n" + Gaps + " gaps between jump(s)."),
                                     NumberLineComplete ? string.Empty : (Overlaps == 0 ? string.Empty : "\n" + Overlaps + " overlapping jump(s)."));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}

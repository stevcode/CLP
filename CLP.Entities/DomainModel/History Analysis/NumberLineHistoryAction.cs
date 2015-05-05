using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    class NumberLineHistoryAction : AHistoryActionBase
    {
        public enum NumberLineActions
        {
            Jump,
            Change,
            InkChange
        }

        #region Constructors
        public NumberLineHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            var jumpSizesChangedActions = NumberLineJumpActions;
            var endPointsChangedActions = NumberLineEndPointsChangedActions;

            if (jumpSizesChangedActions.Any())
            {
                if (endPointsChangedActions.Any())
                {
                    //throw error
                }
                NumberLineAction = NumberLineActions.Jump;
            }
            else
            {
                if (endPointsChangedActions.Count != 1)
                {
                    ///throw error, no objects or too many
                }
                NumberLineAction = NumberLineActions.Change;
            }

        }

        /// <summary>Initializes <see cref="ArrayHistoryAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public NumberLineHistoryAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion //Constructors

        #region Properties

        public NumberLineActions NumberLineAction
        {
            get { return GetValue<NumberLineActions>(NumberLineActionProperty); }
            set { SetValue(NumberLineActionProperty, value); }
        }

        public static readonly PropertyData NumberLineActionProperty = RegisterProperty("NumberLineAction", typeof(NumberLineActions));

        public override string CodedValue
        {
            get
            {
                switch (NumberLineAction)
                {
                    case NumberLineActions.Change:
                        var numberLineEndPointsChangedAction = NumberLineEndPointsChangedActions.First();
                        var oldLength = numberLineEndPointsChangedAction.PreviousEndValue - numberLineEndPointsChangedAction.PreviousStartValue;
                        var newLength = numberLineEndPointsChangedAction.NewEndValue - numberLineEndPointsChangedAction.NewStartValue;
                        return string.Format("NL change[{0}: {1}]", newLength, oldLength);
                    case NumberLineActions.InkChange:
                        return string.Format("NL ink change[{0}: {1}]", 80, 81); //new length, old length
                    case NumberLineActions.Jump:
                        var numberLineJumpActions = NumberLineJumpActions;
                        var startJump = numberLineJumpActions.First();
                        return string.Format("NL jump[{0}: {1}, {2}-{3}]", 42, 7, 0, 42); //numberline jump sizes+start/end values
                         //possibly multiple jump sizes NL jump [70: 7, 0-35, 6, 35-41, 7, 48-55]

                        //possibly off numberline
                    default:
                        return "NL modified";
                }
            }
        }

        #endregion //Properties

        #region Calculated Properties

        public List<NumberLineJumpSizesChangedHistoryItem> NumberLineJumpActions
        {
            get { return HistoryItems.OfType<NumberLineJumpSizesChangedHistoryItem>().ToList(); }
        }

        public List<NumberLineEndPointsChangedHistoryItem> NumberLineEndPointsChangedActions

        {
            get { return HistoryItems.OfType<NumberLineEndPointsChangedHistoryItem>().ToList(); }
        }
        #endregion //Calculated Properties
    }
}

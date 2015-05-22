using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class NumberLineHistoryAction : AHistoryActionBase
    {
        public enum NumberLineActions
        {
            Jump,
            Change,
            InkChange
        }

        #region Constructors
        public NumberLineHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems, string numberLineIdentifier="")
            :base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            CodedID = numberLineIdentifier;
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

        public string CodedID
        {
            get { return GetValue<string>(CodedIDProperty); }
            set { SetValue(CodedIDProperty, value); }
        }

        public static readonly PropertyData CodedIDProperty = RegisterProperty("CodedID", typeof(string));

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
                        var numberLine = ParentPage.GetPageObjectByIDOnPageOrInHistory(numberLineJumpActions.First().NumberLineID) as NumberLine;

                        var currentJumpSize = Int32.MaxValue;
                        var jumpDescriptors = new List<string>();
                        var jumpType = "";
                        foreach (var jumpAction in numberLineJumpActions)
                        {
                            System.Windows.Ink.Stroke stroke = null;
                            
                            if (jumpAction.AddedJumpStrokeIDs.Any())
                            {
                                stroke = ParentPage.GetStrokeByIDOnPageOrInHistory(jumpAction.AddedJumpStrokeIDs.First());
                            }
                            else if (jumpAction.RemovedJumpStrokeIDs.Any())
                            {
                                stroke = ParentPage.GetStrokeByIDOnPageOrInHistory(jumpAction.RemovedJumpStrokeIDs.First());
                                jumpType = " erase";
                            }
                            else
                            {
                                break;
                            }
                            var startPoint = numberLine.GetJumpStartFromStroke(stroke);
                            var endPoint = numberLine.GetJumpEndFromStroke(stroke);
                            var jumpSize = endPoint - startPoint;
                            if (endPoint - startPoint != currentJumpSize)
                            {
                                currentJumpSize = jumpSize;
                                jumpDescriptors.Add(jumpSize.ToString());
                                jumpDescriptors.Add(string.Format("{0}-{1}", startPoint, endPoint));
                            }
                            else
                            {
                                string[] jumpEndpoints = jumpDescriptors.Last().Split(new Char[] { '-' });
                                jumpEndpoints[1] = endPoint.ToString();
                                jumpDescriptors[jumpDescriptors.Count - 1] = string.Join("-", jumpEndpoints);
                            }
                        }

                        return string.Format("NL jump{3}[{0}{2}: {1}]", numberLine.NumberLineSize, string.Join(", ",jumpDescriptors),
                            CodedID, jumpType); //numberline jump sizes+start/end values
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

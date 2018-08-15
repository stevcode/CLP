using System.Linq;

namespace CLP.Entities
{
    public static partial class Codings
    {
        #region Event Types                                                

        #region Ink Event Types

        public const string EVENT_INK_CHANGE = "change";
        public const string EVENT_INK_ADD = "add";
        public const string EVENT_INK_ERASE = "erase";
        public const string EVENT_INK_IGNORE = "ignore";

        #endregion // Ink Event Types

        #region Arith Event Types

        public const string EVENT_ARITH_ADD = "add";
        public const string EVENT_ARITH_ERASE = "erase";

        #endregion // Arith Event Types

        #region General PageObject Event Types

        public const string EVENT_OBJECT_ADD = "add";
        public const string EVENT_OBJECT_MULTIPLE_ADD = "add multiple";
        public const string EVENT_OBJECT_DELETE = "delete";
        public const string EVENT_OBJECT_MULTIPLE_DELETE = "delete multiple";
        public const string EVENT_OBJECT_MOVE = "move";
        public const string EVENT_OBJECT_RESIZE = "resize";

        #endregion // General PageObject Event Types

        #region Array Event Types

        public const string EVENT_ARRAY_DIVIDE = "divide";
        public const string EVENT_ARRAY_DIVIDE_DELETE = "divide delete";
        public const string EVENT_ARRAY_DIVIDE_INK = "divide ink";
        public const string EVENT_ARRAY_DIVIDE_INK_ERASE = "divide ink erase";
        public const string EVENT_ARRAY_ROTATE = "rotate";
        public const string EVENT_ARRAY_SNAP = "snap";
        public const string EVENT_ARRAY_SKIP = "skip";
        public const string EVENT_ARRAY_SKIP_ERASE = "skip erase";
        public const string EVENT_ARRAY_SKIP_PLUS_ARITH = "skip +arith";
        public const string EVENT_ARRAY_EQN = "eqn";
        public const string EVENT_ARRAY_EQN_ERASE = "eqn erase";
        public const string EVENT_ARRAY_COUNT_LINE = "count line";
        public const string EVENT_ARRAY_COUNT_LINE_ERASE = "count line erase";
        public const string EVENT_ARRAY_COUNT_DOT = "count dot";
        public const string EVENT_ARRAY_COUNT_DOT_ERASE = "count dot erase";

        #endregion // Array Event Types

        #region Number Line Event Types

        public const string EVENT_NUMBER_LINE_JUMP = "jump";
        public const string EVENT_NUMBER_LINE_JUMP_ERASE = "jump erase";
        public const string EVENT_NUMBER_LINE_CHANGE = "change";
        public const string EVENT_NUMBER_LINE_CHANGE_INK = "change ink";

        #endregion // Number Line Event Types

        #region Answer Event Types

        public const string EVENT_FILL_IN_ADD = "add";
        public const string EVENT_FILL_IN_ERASE = "erase";
        public const string EVENT_FILL_IN_CHANGE = "change";

        public const string EVENT_MULTIPLE_CHOICE_ADD_PARTIAL = "partial fill in";
        public const string EVENT_MULTIPLE_CHOICE_ADD = "fill in";
        public const string EVENT_MULTIPLE_CHOICE_ADD_ADDITIONAL = "additional fill in";
        public const string EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL = "erase partial";
        public const string EVENT_MULTIPLE_CHOICE_ERASE = "erase";
        public const string EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE = "erase incomplete";

        #endregion // Answer Event Types

        #region IParts Event Types

        public const string EVENT_PARTS_VALUE_CHANGED = "parts changed";

        #endregion // IParts Event Types

        #region ICuttable Event Types

        public const string EVENT_CUT = "cut";

        #endregion // ICuttable Event Types

        #endregion // Event Types

        #region Event Info Variables

        #region Ink Event Info Variables

        public const string EVENT_INFO_INK_LOCATION_NONE = "";
        public const string EVENT_INFO_INK_LOCATION_LEFT = "left of";
        public const string EVENT_INFO_INK_LOCATION_RIGHT = "right of";
        public const string EVENT_INFO_INK_LOCATION_RIGHT_SKIP = "right skip region of";
        public const string EVENT_INFO_INK_LOCATION_TOP = "above";
        public const string EVENT_INFO_INK_LOCATION_BOTTOM = "below";
        public const string EVENT_INFO_INK_LOCATION_BOTTOM_SKIP = "bottom skip region of";
        public const string EVENT_INFO_INK_LOCATION_OVER = "over";
        public const string EVENT_INFO_INK_LOCATION_TOP_LEFT = "left and above";
        public const string EVENT_INFO_INK_LOCATION_TOP_RIGHT = "right and above";
        public const string EVENT_INFO_INK_LOCATION_BOTTOM_LEFT = "left and below";
        public const string EVENT_INFO_INK_LOCATION_BOTTOM_RIGHT = "right and below";

        #endregion // Ink Event Info Variables 

        #region Array Event Info Variables

        public const string EVENT_INFO_ARRAY_CUT_VERTICAL = "v";
        public const string EVENT_INFO_ARRAY_DIVIDER_VERTICAL = "v";

        #endregion // Array Event Info Variables

        #endregion // Event Info Variables

        #region Methods

        public static bool IsFinalAnswerEvent(ISemanticEvent semanticEvent)
        {
            return semanticEvent.CodedObject == OBJECT_FILL_IN || semanticEvent.CodedObject == OBJECT_INTERMEDIARY_FILL_IN || semanticEvent.CodedObject == OBJECT_MULTIPLE_CHOICE;
        }

        // For MC, the bubble the student filled in
        // For FI, the interpretation of the strokes on the page, inside the interpretation region, at that point in history
        public static string GetFinalAnswerEventStudentAnswer(ISemanticEvent semanticEvent)
        {
            if (!IsFinalAnswerEvent(semanticEvent))
            {
                return "[ERROR]: Not Final Answer Event.";
            }

            var eventInfo = semanticEvent.EventInformation;
            var delimiterIndex = eventInfo.LastIndexOf(',');
            var content = new string(eventInfo.Take(delimiterIndex).ToArray());
            if (semanticEvent.CodedObject == OBJECT_FILL_IN)
            {
                content = content.Split(';').Last().Trim();
            }

            return content;
        }

        public static string GetFinalAnswerEventCorrectness(ISemanticEvent semanticEvent)
        {
            if (!IsFinalAnswerEvent(semanticEvent))
            {
                return "[ERROR]: Not Final Answer Event.";
            }

            var eventInfo = semanticEvent.EventInformation;
            var delimiterIndex = eventInfo.LastIndexOf(',');
            var correctness = new string(eventInfo.Skip(delimiterIndex + 2).ToArray());
            return correctness;
        }

        public static bool IsRepresentationEvent(ISemanticEvent semanticEvent)
        {
            return semanticEvent.CodedObject == OBJECT_ARRAY ||
                   semanticEvent.CodedObject == OBJECT_NUMBER_LINE ||
                   semanticEvent.CodedObject == OBJECT_STAMP ||
                   semanticEvent.CodedObject == OBJECT_STAMPED_OBJECT ||
                   semanticEvent.CodedObject == OBJECT_BINS;
        }

        public static bool IsMultipleChoiceEventAnErase(ISemanticEvent semanticEvent)
        {
            return semanticEvent.EventType == EVENT_MULTIPLE_CHOICE_ERASE ||
                   semanticEvent.EventType == EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                   semanticEvent.EventType == EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE;
        }

        #endregion // Methods
    }
}

using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class Codings
    {
        #region Errors

        public const string OBJECT_ERROR = "ERROR";

        public const string ERROR_TYPE_NULL_PAGE_OBJECT = "null pageObject";
        public const string ERROR_TYPE_NULL_STROKE = "null stroke";
        public const string ERROR_TYPE_EMPTY_LIST = "empty list";
        public const string ERROR_TYPE_MIXED_LIST = "mixed list";
        public const string ERROR_TYPE_MULTIPLE_CHOICE_STATUS_INCONSISTANCY = "inconsistant multiple choice status";
        public const string ERROR_TYPE_EMPTY_BUFFER = "empty buffer";
        public const string ERROR_TYPE_MIXED_BUFFER = "mixed buffer";

        #endregion // Errors

        #region Coded Objects

        public const string OBJECT_NOTHING = "NOTHING";
        public const string OBJECT_PAGE_OBJECTS = "OBJECTS";
        public const string OBJECT_INK = "INK";
        public const string OBJECT_ARITH = "ARITH";
        public const string OBJECT_SHAPE = "SHAPE";
        public const string OBJECT_ARRAY = "ARR";
        public const string OBJECT_DIVISION_TEMPLATE = "DT";
        public const string OBJECT_REMAINDER_TILES = "TILES";
        public const string OBJECT_NUMBER_LINE = "NL";
        public const string OBJECT_STAMP = "STAMP";
        public const string OBJECT_STAMPED_OBJECTS = "STAMP IMAGES";
        public const string OBJECT_BINS = "BINS";
        public const string OBJECT_FILL_IN = "ANS FI";
        public const string OBJECT_MULTIPLE_CHOICE = "ANS MC";
        public const string OBJECT_TEXT = "TEXT";

        public static readonly Dictionary<string, string> FriendlyObjects = new Dictionary<string, string>
                                                                            {
                                                                                { OBJECT_NOTHING, "Nothing" },
                                                                                { OBJECT_PAGE_OBJECTS, "PageObjects" },
                                                                                { OBJECT_INK, "Ink" },
                                                                                { OBJECT_ARITH, "Arithmetic" },
                                                                                { OBJECT_SHAPE, "Shape" },
                                                                                { OBJECT_ARRAY, "Array" },
                                                                                { OBJECT_DIVISION_TEMPLATE, "Division Template" },
                                                                                { OBJECT_REMAINDER_TILES, "Remainder Tiles" },
                                                                                { OBJECT_NUMBER_LINE, "Number Line" },
                                                                                { OBJECT_STAMP, "Stamp" },
                                                                                { OBJECT_STAMPED_OBJECTS, "Stamp Images" },
                                                                                { OBJECT_BINS, "Bins" },
                                                                                { OBJECT_FILL_IN, "Final Answer Fill In" },
                                                                                { OBJECT_MULTIPLE_CHOICE, "Final Answer Multiple Choice" },
                                                                                { OBJECT_TEXT, "Text" }
                                                                            };

        public static string CorrectnessToCodedCorrectness(Correctness correctness)
        {
            switch (correctness)
            {
                case Correctness.Correct:
                    return CORRECTNESS_CORRECT;
                case Correctness.PartiallyCorrect:
                    return CORRECTNESS_PARTIAL;
                case Correctness.Incorrect:
                    return CORRECTNESS_INCORRECT;
                case Correctness.Illegible:
                    return CORRECTNESS_ILLEGIBLE;
                default:
                    return CORRECTNESS_UNKNOWN;
            }
        }

        public static string CorrectnessToFriendlyCorrectness(Correctness correctness)
        {
            var codedCorrectness = CorrectnessToCodedCorrectness(correctness);
            var friendlyCorrectness = CodedCorrectnessToFriendlyCorrectness(codedCorrectness);

            return friendlyCorrectness;
        }

        public static string CodedCorrectnessToFriendlyCorrectness(string codedCorrectness)
        {
            switch (codedCorrectness)
            {
                case CORRECTNESS_CORRECT:
                    return "Correct";
                case CORRECTNESS_PARTIAL:
                    return "Partially Correct";
                case CORRECTNESS_INCORRECT:
                    return "Incorrect";
                case CORRECTNESS_ILLEGIBLE:
                    return "Illegible";
                default:
                    return "Unknown";
            }
        }

        public static Correctness CodedCorrectnessToCorrectness(string codedCorrectness)
        {
            switch (codedCorrectness)
            {
                case CORRECTNESS_CORRECT:
                    return Correctness.Correct;
                case CORRECTNESS_PARTIAL:
                    return Correctness.PartiallyCorrect;
                case CORRECTNESS_INCORRECT:
                    return Correctness.Incorrect;
                case CORRECTNESS_ILLEGIBLE:
                    return Correctness.Illegible;
                default:
                    return Correctness.Unknown;
            }
        }

        #endregion // Coded Objects

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

        #region Analysis Codes

        #region Representation Sequence

        public const string ANALYSIS_COR_BEFORE_REP = "ABR-C";
        public const string ANALYSIS_INC_BEFORE_REP = "ABR-I";
        public const string ANALYSIS_INC_TO_COR_AFTER_REP = "ARIC";
        public const string ANALYSIS_COR_TO_INC_AFTER_REP = "ARCI";
        public const string ANALYSIS_COR_TO_COR_AFTER_REP = "ARCC";
        public const string ANALYSIS_INC_TO_INC_AFTER_REP = "ARII";
        public const string ANALYSIS_REP_AFTER_ANSWER = "RAA";

        #endregion // Representation Sequence

        #region Strategies

        public const string STRATEGY_NAME_ARRAY_COUNT_BY_ONE = "COUNT-BY-ONE";
        public const string STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT = "PART";
        public const string STRATEGY_NAME_ARRAY_SKIP = "SKIP";

        public const string STRATEGY_NAME_NUMBER_LINE_REPEAT_ADDITION = "REPEAT ADD";

        public const string STRATEGY_NAME_BINS_DEAL = "DEAL";

        #endregion // Strategies

        #region Strategy Specifics

        public const string STRATEGY_SPECIFICS_ARRAY_CUT = "cut";
        public const string STRATEGY_SPECIFICS_ARRAY_SNAP = "snap";
        public const string STRATEGY_SPECIFICS_ARRAY_CUT_SNAP = "cut and snap";
        public const string STRATEGY_SPECIFICS_ARRAY_DIVIDE = "divide";
        public const string STRATEGY_SPECIFICS_ARRAY_DIVIDE_INK = "ink divide";
        public const string STRATEGY_SPECIFICS_ARRAY_ARITH = "+arith";
        public const string STRATEGY_SPECIFICS_ARRAY_DOTS = "dots";

        #endregion // Strategy Specifics

        #region Misc

        public const string NUMBER_LINE_NLJE = "NLJE";

        public const string NUMBER_LINE_BLANK_PARTIAL_MATCH = "NLBP";

        public const string REPRESENTATIONS_MR = "MR";
        public const string REPRESENTATIONS_MR2STEP = "MR2STEP";

        #endregion // Misc

        #endregion // Analysis Codes

        #region Correctness

        public const string CORRECTNESS_CORRECT = "COR";
        public const string CORRECTNESS_PARTIAL = "PAR";
        public const string CORRECTNESS_INCORRECT = "INC";
        public const string CORRECTNESS_ILLEGIBLE = "ILL";
        public const string CORRECTNESS_UNKNOWN = "UNKNOWN";

        public const string ANSWER_UNDEFINED = "UNDEFINED";

        public const string MATCHED_RELATION_LEFT = "LS";
        public const string MATCHED_RELATION_RIGHT = "RS";
        public const string MATCHED_RELATION_ALTERNATIVE = "ALT";
        public const string MATCHED_RELATION_NONE = "NONE";

        public const string PARTIAL_REASON_UNKNOWN = "UNKNOWN";
        public const string PARTIAL_REASON_SWAPPED = "SWAPPED";

        #endregion // Correctness

        #region Methods

        public static bool IsFinalAnswerEvent(ISemanticEvent semanticEvent)
        {
            return semanticEvent.CodedObject == OBJECT_FILL_IN || semanticEvent.CodedObject == OBJECT_MULTIPLE_CHOICE;
        }

        public static string GetFinalAnswerEventContent(ISemanticEvent semanticEvent)
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
            return semanticEvent.CodedObject == OBJECT_ARRAY || semanticEvent.CodedObject == OBJECT_NUMBER_LINE || semanticEvent.CodedObject == OBJECT_STAMP ||
                   semanticEvent.CodedObject == OBJECT_STAMPED_OBJECTS || semanticEvent.CodedObject == OBJECT_BINS;
        }

        #endregion // Methods
    }
}
using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class Codings
    {
        #region CodedObjects

        public const string OBJECT_INK = "INK";
        public const string OBJECT_ARITH = "ARITH";
        public const string OBJECT_ARRAY = "ARR";
        public const string OBJECT_DIVISION_TEMPLATE = "DT";
        public const string OBJECT_REMAINDER_TILES = "TILES";
        public const string OBJECT_NUMBER_LINE = "NL";
        public const string OBJECT_STAMP = "STAMP";
        public const string OBJECT_STAMPED_OBJECTS = "STAMP IMAGES";
        public const string OBJECT_BINS = "BINS";
        public const string OBJECT_TEXT = "TEXT";
        public const string OBJECT_FILL_IN = "ANS FI";
        public const string OBJECT_MULTIPLE_CHOICE = "ANS MC";

        public static readonly Dictionary<string, string> FriendlyObjects = new Dictionary<string, string>
                                                                            {
                                                                                { OBJECT_INK, "Ink" },
                                                                                { OBJECT_ARRAY, "Array" },
                                                                                { OBJECT_STAMP, "Stamp" },
                                                                                { OBJECT_STAMPED_OBJECTS, "Stamp Images" },
                                                                                { OBJECT_NUMBER_LINE, "Number Line" },
                                                                                { OBJECT_FILL_IN, "Filled In Answer" },
                                                                                { OBJECT_MULTIPLE_CHOICE, "Multiple Choice" }
                                                                            };

        #endregion // CodedObjects

        #region CodedActions                                                        // {Variable} (Optional)                                                

        #region Ink Actions

        public const string ACTION_INK_CHANGE = "change"; // ID = string.Empty;
        public const string ACTION_INK_IGNORE = "ignore"; // ID = string.Empty;
        public const string ACTION_INK_ADD = "strokes"; // ActionID = "{ActionIDInkLocation} {CodedObject} [{CodedID} {IncrementID}]"
        public const string ACTION_INK_ERASE = "strokes erase"; // ActionID = "{ActionIDInkLocation} {CodedObject} [{CodedID} {IncrementID}]"

        #endregion // Ink Actions

        #region Arith Actions

        public const string ACTION_ARITH_ADD = "add";
        public const string ACTION_ARITH_ERASE = "erase";

        #endregion // Arith Actions

        #region Dot Actions

        // TODO: actually define correctly.
        public const string ACTION_DOTS_ADD = "add";
        public const string ACTION_DOTS_ERASE = "erase";

        #endregion // Dot Actions

        #region Answer Actions

        public const string ACTION_FILL_IN_ADD = "add";
        public const string ACTION_FILL_IN_ERASE = "erase";
        public const string ACTION_MULTIPLE_CHOICE_ADD_PARTIAL = "partial";
        public const string ACTION_MULTIPLE_CHOICE_ADD = "fill in";
        public const string ACTION_MULTIPLE_CHOICE_ADD_ADDITIONAL = "additional";
        public const string ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL = "erase partial";
        public const string ACTION_MULTIPLE_CHOICE_ERASE = "erase";
        public const string ACTION_MULTIPLE_CHOICE_ERASE_INCOMPLETE = "erase incomplete";

        #endregion // Answer Actions

        #region General PageObject Actions

        public const string ACTION_OBJECT_ADD = "add";
        public const string ACTION_OBJECT_DELETE = "delete";
        public const string ACTION_OBJECT_MOVE = "move";
        public const string ACTION_OBJECT_RESIZE = "resize";

        #endregion // General PageObject Actions

        #region Number Line Actions

        public const string ACTION_NUMBER_LINE_JUMP = "jump";
                            // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}(ACTIONID_NUMBER_LINE_JUMP_RUNOFF)(; REPEAT; REPEAT)"   // ACTIONID_NUMBER_LINE_JUMP_RUNOFF replaces {EndTick} if arc of Jump goes past edge of Number Line.

        public const string ACTION_NUMBER_LINE_JUMP_ERASE = "jump erase";

        public const string ACTION_NUMBER_LINE_JUMP_BELOW = "jump below";
                            // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}(ACTIONID_NUMBER_LINE_JUMP_RUNOFF)(; REPEAT; REPEAT)"   // ACTIONID_NUMBER_LINE_JUMP_RUNOFF replaces {EndTick} if arc of Jump goes past edge of Number Line.

        public const string ACTION_NUMBER_LINE_CHANGE = "change"; // ActionID = "{NewNumberLineSize} {IncrementID}"
        public const string ACTION_NUMBER_LINE_CHANGE_INK = "change ink"; // ActionID = "{NewNumberLineSize} {IncrementID}"

        #endregion // Number Line Actions

        #region Array Actions

        public const string ACTION_ARRAY_CUT = "cut"; // ActionID = "{NewArrayCodedID} {IncrementID}, {NewArrayCodedID} {IncrementID}(, ACTIONID_ARRAY_CUT_VERTICAL)"
        public const string ACTION_ARRAY_DIVIDE = "divide"; // ActionID = "{SubArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}(, REPEAT)(, ACTIONID_ARRAY_DIVIDER_VERTICAL)"
        public const string ACTION_ARRAY_DIVIDE_INK = "divide ink"; // ActionID = "{SubArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}(, REPEAT)(, ACTIONID_ARRAY_DIVIDER_VERTICAL)"
        public const string ACTION_ARRAY_DIVIDE_INK_ERASE = "divide ink erase";
        public const string ACTION_ARRAY_ROTATE = "rotate"; // ActionID = "{ArrayCodedID} {IncrementID}"
        public const string ACTION_ARRAY_SNAP = "snap"; // ID = "{ArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}", ActionID = "{ArrayCodedID} {IncrementID}"
        public const string ACTION_ARRAY_SKIP = "skip";
        public const string ACTION_ARRAY_SKIP_ERASE = "skip erase";
        public const string ACTION_ARRAY_EQN = "eqn";
        public const string ACTION_ARRAY_EQN_ERASE = "eqn erase";

        #endregion // Array Actions

        #endregion // CodedActions

        #region CodedActionIDVariables

        #region Ink ActionID Variables

        public const string ACTIONID_INK_LOCATION_NONE = "";
        public const string ACTIONID_INK_LOCATION_LEFT = "left of";
        public const string ACTIONID_INK_LOCATION_RIGHT = "right of";
        public const string ACTIONID_INK_LOCATION_RIGHT_SKIP = "right skip region of";
        public const string ACTIONID_INK_LOCATION_TOP = "above";
        public const string ACTIONID_INK_LOCATION_BOTTOM = "below";
        public const string ACTIONID_INK_LOCATION_OVER = "over";
        public const string ACTIONID_INK_LOCATION_TOP_LEFT = "left and above";
        public const string ACTIONID_INK_LOCATION_TOP_RIGHT = "right and above";
        public const string ACTIONID_INK_LOCATION_BOTTOM_LEFT = "left and below";
        public const string ACTIONID_INK_LOCATION_BOTTOM_RIGHT = "right and below";

        #endregion // Ink ActionID Variables 

        #region Number Line ActionID Variables

        public const string ACTIONID_NUMBER_LINE_JUMP_RUNOFF = "off NL";

        #endregion // Number Line ActionID Variables

        #region Array ActionID Variables

        public const string ACTIONID_ARRAY_CUT_VERTICAL = "v";
        public const string ACTIONID_ARRAY_DIVIDER_VERTICAL = "v";

        #endregion // Array ActionID Variables

        #endregion // CodedActionIDVariables

        #region Meta Data

        public const string META_REFERENCE_PAGE_OBJECT_ID = "REFERENCE_PAGE_OBJECT_ID";

        #endregion // Meta Data

        #region Analysis Codes

        public const string ANALYSIS_COR_BEFORE_REP = "ABR-C";
        public const string ANALYSIS_INC_BEFORE_REP = "ABR-I";
        public const string ANALYSIS_INC_TO_COR_AFTER_REP = "ARIC";
        public const string ANALYSIS_COR_TO_INC_AFTER_REP = "ARCI";
        public const string ANALYSIS_COR_TO_COR_AFTER_REP = "ARCC";
        public const string ANALYSIS_INC_TO_INC_AFTER_REP = "ARII";

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

        #endregion // Analysis Codes

        #region Correctness

        public const string CORRECTNESS_CORRECT = "COR";
        public const string CORRECTNESS_PARTIAL = "PAR";
        public const string CORRECTNESS_INCORRECT = "INC";

        #endregion // Correctness

        public static bool IsAnswerObject(IHistoryAction historyAction) { return historyAction.CodedObject == OBJECT_FILL_IN || historyAction.CodedObject == OBJECT_MULTIPLE_CHOICE; }

        public static string GetAnswerObjectContent(IHistoryAction historyAction)
        {
            if (!IsAnswerObject(historyAction))
            {
                return "[ERROR]: Not Answer Object.";
            }

            var actionID = historyAction.CodedObjectActionID;
            var delimiterIndex = actionID.LastIndexOf(',');
            var content = new string(actionID.Take(delimiterIndex).ToArray());
            return content;
        }

        public static string GetAnswerObjectCorrectness(IHistoryAction historyAction)
        {
            if (!IsAnswerObject(historyAction))
            {
                return "[ERROR]: Not Answer Object.";
            }

            var actionID = historyAction.CodedObjectActionID;
            var delimiterIndex = actionID.LastIndexOf(',');
            var correctness = new string(actionID.Skip(delimiterIndex + 2).ToArray());
            return correctness;
        }

        public static bool IsRepresentationObject(IHistoryAction historyAction)
        {
            return historyAction.CodedObject == OBJECT_ARRAY || historyAction.CodedObject == OBJECT_NUMBER_LINE || historyAction.CodedObject == OBJECT_STAMP ||
                   historyAction.CodedObject == OBJECT_STAMPED_OBJECTS || historyAction.CodedObject == OBJECT_BINS;
        }
    }
}
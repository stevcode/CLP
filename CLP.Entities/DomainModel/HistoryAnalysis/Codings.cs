namespace CLP.Entities
{
    public static class Codings
    {
        #region CodedObjects

        public const string OBJECT_INK = "INK";
        public const string OBJECT_ARRAY = "ARR";
        public const string OBJECT_NUMBER_LINE = "NL";
        public const string OBJECT_STAMP = "STAMP";
        public const string OBJECT_STAMPED_OBJECTS = "STAMP IMAGES";
        public const string OBJECT_GROUP = "GROUP";
        public const string OBJECT_BINS = "BINS";
        public const string OBJECT_TEXT = "TEXT";

        #endregion // CodedObjects

        #region CodedActions                                                        // {Variable} (Optional)                                                

        #region Ink Actions

        public const string ACTION_INK_CHANGE = "change";                           // ID = string.Empty;
        public const string ACTION_INK_IGNORE = "ignore";                           // ID = string.Empty;
        public const string ACTION_INK_ADD = "strokes";                             // ActionID = "{ActionIDInkLocation} {CodedObject} [{CodedID} {IncrementID}]"
        public const string ACTION_INK_ERASE = "strokes erase";                     // ActionID = "{ActionIDInkLocation} {CodedObject} [{CodedID} {IncrementID}]"

        #endregion // Ink Actions

        #region General PageObject Actions

        public const string ACTION_OBJECT_ADD = "add";
        public const string ACTION_OBJECT_DELETE = "delete";
        public const string ACTION_OBJECT_MOVE = "move";
        public const string ACTION_OBJECT_RESIZE = "resize";

        #endregion // General PageObject Actions

        #region Number Line Actions

        public const string ACTION_NUMBER_LINE_JUMP = "jump";                       // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}(ACTIONID_NUMBER_LINE_JUMP_RUNOFF)(; REPEAT; REPEAT)"   // ACTIONID_NUMBER_LINE_JUMP_RUNOFF replaces {EndTick} if arc of Jump goes past edge of Number Line.
        public const string ACTION_NUMBER_LINE_JUMP_BELOW = "jump below";           // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}(ACTIONID_NUMBER_LINE_JUMP_RUNOFF)(; REPEAT; REPEAT)"   // ACTIONID_NUMBER_LINE_JUMP_RUNOFF replaces {EndTick} if arc of Jump goes past edge of Number Line.
        public const string ACTION_NUMBER_LINE_CHANGE = "change";                   // ActionID = "{NewNumberLineSize} {IncrementID}"
        public const string ACTION_NUMBER_LINE_CHANGE_INK = "change ink";           // ActionID = "{NewNumberLineSize} {IncrementID}"

        #endregion // Number Line Actions

        #region Array Actions

        public const string ACTION_ARRAY_CUT = "cut";                               // ActionID = "{NewArrayCodedID} {IncrementID}, {NewArrayCodedID} {IncrementID}(, ACTIONID_ARRAY_CUT_VERTICAL)"
        public const string ACTION_ARRAY_DIVIDE = "divide";                         // ActionID = "{SubArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}(, REPEAT)(, ACTIONID_ARRAY_DIVIDER_VERTICAL)"
        public const string ACTION_ARRAY_DIVIDE_INK = "divide ink";                 // ActionID = "{SubArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}(, REPEAT)(, ACTIONID_ARRAY_DIVIDER_VERTICAL)"
        public const string ACTION_ARRAY_ROTATE = "rotate";                         // ActionID = "{ArrayCodedID} {IncrementID}"
        public const string ACTION_ARRAY_SNAP = "snap";                             // ID = "{ArrayCodedID} {IncrementID}, {SubArrayCodedID} {IncrementID}", ActionID = "{ArrayCodedID} {IncrementID}"

        #endregion // Array Actions

        #endregion // CodedActions

        #region CodedActionIDVariables

        #region Ink ActionID Variables

        public const string ACTIONID_INK_LOCATION_NONE = "";
        public const string ACTIONID_INK_LOCATION_LEFT = "left of";
        public const string ACTIONID_INK_LOCATION_RIGHT = "right of";
        public const string ACTIONID_INK_LOCATION_TOP = "above";
        public const string ACTIONID_INK_LOCATION_BOTTOM = "below";
        public const string ACTIONID_INK_LOCATION_OVER = "over";

        #endregion // Ink ActionID Variables 

        #region Number Line ActionID Variables

        public const string ACTIONID_NUMBER_LINE_JUMP_RUNOFF = "off NL";

        #endregion // Number Line ActionID Variables

        #region Array ActionID Variables

        public const string ACTIONID_ARRAY_CUT_VERTICAL = "v";
        public const string ACTIONID_ARRAY_DIVIDER_VERTICAL = "v";

        #endregion // Array ActionID Variables

        #endregion // CodedActionIDVariables
    }
}
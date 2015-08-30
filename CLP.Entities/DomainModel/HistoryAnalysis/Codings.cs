namespace CLP.Entities
{
    public static class Codings
    {
        #region CodedObjects

        public const string OBJECT_INK = "INK";
        public const string OBJECT_ARRAY = "ARR";
        public const string OBJECT_NUMBER_LINE = "NL";
        public const string OBJECT_BINS = "BINS";
        public const string OBJECT_STAMP = "STAMP";
        public const string OBJECT_STAMPED_OBJECTS = "STAMP IMAGES";
        public const string OBJECT_GROUP = "GROUP";

        #endregion // CodedObjects

        #region CodedActions                                                        
                                                                                    // {Variable} (Optional)
        #region General PageObject Actions

        public const string ACTION_OBJECT_ADD = "add";
        public const string ACTION_OBJECT_DELETE = "delete";
        public const string ACTION_OBJECT_MOVE = "move";
        public const string ACTION_OBJECT_RESIZE = "resize";

        #endregion // General PageObject Actions

        #region Number Line Actions

        public const string ACTION_NUMBER_LINE_JUMP = "jump";                       // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}("off NL")(; REPEAT; REPEAT)"   // "off NL" replaces {EndTick} if arc of Jump goes past edge of Number Line.
        public const string ACTION_NUMBER_LINE_JUMP_BELOW = "jump below";           // ActionID = "{JumpSizeOfIdenticalConsecutiveJumps}, {StartTick}-{EndTick}("off NL")(; REPEAT; REPEAT)"   // "off NL" replaces {EndTick} if arc of Jump goes past edge of Number Line.
        public const string ACTION_NUMBER_LINE_CHANGE = "change";                   // ActionID = "{NewNumberLineSize} {IncrementID}"
        public const string ACTION_NUMBER_LINE_CHANGE_INK = "change ink";           // ActionID = "{NewNumberLineSize} {IncrementID}"

        #endregion // Number Line Actions

        #region Array Actions

        public const string ACTION_ARRAY_CUT = "cut";
        public const string ACTION_ARRAY_DIVIDE = "divide";
        public const string ACTION_ARRAY_DIVIDE_INK = "divide ink";
        public const string ACTION_ARRAY_ROTATE = "rotate";
        public const string ACTION_ARRAY_SNAP = "snap";

        #endregion // Array Actions

        #endregion // CodedActions
    }
}
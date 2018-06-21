using System.Collections.Generic;

namespace CLP.Entities
{
    public static partial class Codings
    {
        #region Generic

        public const string NOT_APPLICABLE = "NA";

        #endregion // Generic

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
        public const string OBJECT_STAMPED_OBJECT = "STAMP IMAGE";
        public const string OBJECT_BINS = "BINS";
        public const string OBJECT_FILL_IN = "ANS FI";
        public const string OBJECT_INTERMEDIARY_FILL_IN = "INT ANS FI";
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
                                                                                { OBJECT_STAMPED_OBJECT, "Stamp Image" },
                                                                                { OBJECT_BINS, "Bins" },
                                                                                { OBJECT_FILL_IN, "Final Answer Fill In" },
                                                                                { OBJECT_INTERMEDIARY_FILL_IN, "Intermediary Answer Fill In" },
                                                                                { OBJECT_MULTIPLE_CHOICE, "Final Answer Multiple Choice" },
                                                                                { OBJECT_TEXT, "Text" }
                                                                            };

        

        #endregion // Coded Objects
    }
}
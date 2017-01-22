﻿using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class Codings
    {
        #region Coded Objects

        public const string OBJECT_NOTHING = "NOTHING";
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
                                                                                { OBJECT_FILL_IN, "Filled In Answer" },
                                                                                { OBJECT_MULTIPLE_CHOICE, "Multiple Choice" },
                                                                                { OBJECT_TEXT, "Text" },
                                                                            };

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
        public const string EVENT_OBJECT_DELETE = "delete";
        public const string EVENT_OBJECT_MOVE = "move";
        public const string EVENT_OBJECT_RESIZE = "resize";

        #endregion // General PageObject Event Types

        #region Array Event Types

        public const string EVENT_ARRAY_CUT = "cut";
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
        public const string EVENT_MULTIPLE_CHOICE_ADD_PARTIAL = "partial fill in";
        public const string EVENT_MULTIPLE_CHOICE_ADD = "fill in";
        public const string EVENT_MULTIPLE_CHOICE_ADD_ADDITIONAL = "additional fill in";
        public const string EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL = "erase partial";
        public const string EVENT_MULTIPLE_CHOICE_ERASE = "erase";
        public const string EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE = "erase incomplete";

        #endregion // Answer Event Types

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

        #endregion // Analysis Codes

        #region Correctness

        public const string CORRECTNESS_CORRECT = "COR";
        public const string CORRECTNESS_PARTIAL = "PAR";
        public const string CORRECTNESS_INCORRECT = "INC";

        #endregion // Correctness

        #region Methods

        public static bool IsAnswerObject(ISemanticEvent semanticEvent)
        {
            return semanticEvent.CodedObject == OBJECT_FILL_IN || semanticEvent.CodedObject == OBJECT_MULTIPLE_CHOICE;
        }

        public static string GetAnswerObjectContent(ISemanticEvent semanticEvent)
        {
            if (!IsAnswerObject(semanticEvent))
            {
                return "[ERROR]: Not Answer Object.";
            }

            var eventInfo = semanticEvent.EventInformation;
            var delimiterIndex = eventInfo.LastIndexOf(',');
            var content = new string(eventInfo.Take(delimiterIndex).ToArray());
            return content;
        }

        public static string GetAnswerObjectCorrectness(ISemanticEvent semanticEvent)
        {
            if (!IsAnswerObject(semanticEvent))
            {
                return "[ERROR]: Not Answer Object.";
            }

            var eventInfo = semanticEvent.EventInformation;
            var delimiterIndex = eventInfo.LastIndexOf(',');
            var correctness = new string(eventInfo.Skip(delimiterIndex + 2).ToArray());
            return correctness;
        }

        public static bool IsRepresentationObject(ISemanticEvent semanticEvent)
        {
            return semanticEvent.CodedObject == OBJECT_ARRAY || semanticEvent.CodedObject == OBJECT_NUMBER_LINE || semanticEvent.CodedObject == OBJECT_STAMP ||
                   semanticEvent.CodedObject == OBJECT_STAMPED_OBJECTS || semanticEvent.CodedObject == OBJECT_BINS;
        }

        #endregion // Methods
    }
}
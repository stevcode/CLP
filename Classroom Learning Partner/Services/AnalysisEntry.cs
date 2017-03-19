using System.Collections.Generic;
using Catel.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class AnalysisEntry : AEntityBase
    {
        #region Constants

        public const string YES = "Y";
        public const string NO = "N";
        public const string UNKOWN_ERROR = "UE";
        public const string UNSUBMITTED = "UNSUBMITTED";
        public const string NONE = "None";
        public const string NA = "N/A";

        public const string CORRECTNESS_UNKNOWN = "U";
        public const string CORRECTNESS_CORRECT = "C";
        public const string CORRECTNESS_INCORRECT = "I";
        public const string CORRECTNESS_PARTIAL = "P";
        public const string CORRECTNESS_ILLEGIBLE = "ILL";
        public const string CORRECTNESS_UNANSWERED = "UNA";

        public const string PROBLEM_TYPE_1_PART = "1P";
        public const string PROBLEM_TYPE_2_PART = "2P";
        public const string PROBLEM_TYPE_EQUIVALENCE = "E";

        public const string WORD_TYPE_WORD = "W";
        public const string WORD_TYPE_NON_WORD = "NW";

        public const string OPERATION_TYPE_MULTIPLICATION_MISSING_NONE = "M1";
        public const string OPERATION_TYPE_MULTIPLICATION_MISSING_LAST_FACTOR = "M2";
        public const string OPERATION_TYPE_MULTIPLICATION_MISSING_FIRST_FACTOR = "M3";
        public const string OPERATION_TYPE_DIVISION_MISSING_NONE = "D1";
        public const string OPERATION_TYPE_DIVISION_MISSING_DIVISOR = "D2";
        public const string OPERATION_TYPE_DIVISION_MISSING_DIVIDEND = "D3";

        public const string DIVISION_TYPE_GENERAL = "N/A";
        public const string DIVISION_TYPE_PARTATIVE = "P";
        public const string DIVISION_TYPE_QUOTATIVE = "Q";

        public const string DIFFICULTY_LEVEL_NONE = "N";
        public const string DIFFICULTY_LEVEL_EASY = "E";
        public const string DIFFICULTY_LEVEL_MEDIUM = "M";
        public const string DIFFICULTY_LEVEL_HARD = "H";

        public const string SPECIAL_INTEREST_GROUP_CE = "CE";
        public const string SPECIAL_INTEREST_GROUP_ZERO = "Zero";
        public const string SPECIAL_INTEREST_GROUP_SCAF = "SCAF";
        public const string SPECIAL_INTEREST_GROUP_2PSF = "2PSF";
        public const string SPECIAL_INTEREST_GROUP_2PSS = "2PSS";

        #endregion // Constants

        #region Constructors

        public AnalysisEntry(string ownerName, int pageNumber)
        {
            OwnerName = ownerName;
            PageNumber = pageNumber;
        }

        #endregion // Constructors

        #region Properties

        #region Page Identification

        /// <summary>Full Name of the owner of the page.</summary>
        public string OwnerName
        {
            get { return GetValue<string>(OwnerNameProperty); }
            set { SetValue(OwnerNameProperty, value); }
        }

        public static readonly PropertyData OwnerNameProperty = RegisterProperty("OwnerName", typeof(string), string.Empty);

        /// <summary>Page Number of the page.</summary>
        public int PageNumber
        {
            get { return GetValue<int>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int), 0);

        /// <summary>Time the page was most recently submitted. Marked UNSUBMITTED if never submitted.</summary>
        public string SubmissionTime
        {
            get { return GetValue<string>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(string), string.Empty);

        #endregion // Page Identification

        #region Problem Characteristics

        /// <summary>Signifies the type of problem given in the page definition.</summary>
        public string ProblemType
        {
            get { return GetValue<string>(ProblemTypeProperty); }
            set { SetValue(ProblemTypeProperty, value); }
        }

        public static readonly PropertyData ProblemTypeProperty = RegisterProperty("ProblemType", typeof(string), string.Empty);

        /// <summary>Signifies the problem on the page was either a Word Problem or Non-Word Problem.</summary>
        public string WordType
        {
            get { return GetValue<string>(WordTypeProperty); }
            set { SetValue(WordTypeProperty, value); }
        }

        public static readonly PropertyData WordTypeProperty = RegisterProperty("WordType", typeof(string), string.Empty);

        /// <summary>Designates left side's (or only side's, if no right side) multiplication/division operation type.</summary>
        public string LeftSideOperation
        {
            get { return GetValue<string>(LeftSideOperationProperty); }
            set { SetValue(LeftSideOperationProperty, value); }
        }

        public static readonly PropertyData LeftSideOperationProperty = RegisterProperty("LeftSideOperation", typeof(string), string.Empty);

        /// <summary>Designates right side's multiplication/division operation type.</summary>
        public string RightSideOperation
        {
            get { return GetValue<string>(RightSideOperationProperty); }
            set { SetValue(RightSideOperationProperty, value); }
        }

        public static readonly PropertyData RightSideOperationProperty = RegisterProperty("RightSideOperation", typeof(string), string.Empty);

        /// <summary>Type of division if 1-Part division problem.</summary>
        public string DivisionType
        {
            get { return GetValue<string>(DivisionTypeProperty); }
            set { SetValue(DivisionTypeProperty, value); }
        }

        public static readonly PropertyData DivisionTypeProperty = RegisterProperty("DivisionType", typeof(string), string.Empty);

        /// <summary>Signifies the page uses a Multiple Choice box for the final answer.</summary>
        public string IsMultipleChoiceBoxOnPage
        {
            get { return GetValue<string>(IsMultipleChoiceBoxOnPageProperty); }
            set { SetValue(IsMultipleChoiceBoxOnPageProperty, value); }
        }

        public static readonly PropertyData IsMultipleChoiceBoxOnPageProperty = RegisterProperty("IsMultipleChoiceBoxOnPage", typeof(string), string.Empty);

        /// <summary>Signifies the difficulty level of the page, if such a difficulty level was applied.</summary>
        public string DifficultyLevel
        {
            get { return GetValue<string>(DifficultyLevelProperty); }
            set { SetValue(DifficultyLevelProperty, value); }
        }

        public static readonly PropertyData DifficultyLevelProperty = RegisterProperty("DifficultyLevel", typeof(string), string.Empty);

        /// <summary>Equation designated by the page definition.</summary>
        public string PageDefinitionEquation
        {
            get { return GetValue<string>(PageDefinitionEquationProperty); }
            set { SetValue(PageDefinitionEquationProperty, value); }
        }

        public static readonly PropertyData PageDefinitionEquationProperty = RegisterProperty("PageDefinitionEquation", typeof(string), string.Empty);

        /// <summary>Lists representations required by the text of the problem, otherwise None.</summary>
        public string RequiredRepresentations
        {
            get { return GetValue<string>(RequiredRepresentationsProperty); }
            set { SetValue(RequiredRepresentationsProperty, value); }
        }

        public static readonly PropertyData RequiredRepresentationsProperty = RegisterProperty("RequiredRepresentations", typeof(string), string.Empty);
        
        /// <summary>Reaseracher specified list of types of special interest groups the page may be part of.</summary>
        public List<string> SpecialInterestGroups
        {
            get { return GetValue<List<string>>(SpecialInterestGroupsProperty); }
            set { SetValue(SpecialInterestGroupsProperty, value); }
        }

        public static readonly PropertyData SpecialInterestGroupsProperty = RegisterProperty("SpecialInterestGroups", typeof(List<string>), () => new List<string>());
        
        #endregion // Problem Characteristics

        #region Whole Page Characteristics

        /// <summary>SUMMARY</summary>
        public string IsInkOnly
        {
            get { return GetValue<string>(IsInkOnlyProperty); }
            set { SetValue(IsInkOnlyProperty, value); }
        }

        public static readonly PropertyData IsInkOnlyProperty = RegisterProperty("IsInkOnly", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsBlank
        {
            get { return GetValue<string>(IsBlankProperty); }
            set { SetValue(IsBlankProperty, value); }
        }

        public static readonly PropertyData IsBlankProperty = RegisterProperty("IsBlank", typeof(string), string.Empty);
        
        /// <summary>SUMMARY</summary>
        public int ArrayDeletedCount
        {
            get { return GetValue<int>(ArrayDeletedCountProperty); }
            set { SetValue(ArrayDeletedCountProperty, value); }
        }

        public static readonly PropertyData ArrayDeletedCountProperty = RegisterProperty("ArrayDeletedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int NumberLineCreatedCount
        {
            get { return GetValue<int>(NumberLineCreatedCountProperty); }
            set { SetValue(NumberLineCreatedCountProperty, value); }
        }

        public static readonly PropertyData NumberLineCreatedCountProperty = RegisterProperty("NumberLineCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int NumberLineDeletedCount
        {
            get { return GetValue<int>(NumberLineDeletedCountProperty); }
            set { SetValue(NumberLineDeletedCountProperty, value); }
        }

        public static readonly PropertyData NumberLineDeletedCountProperty = RegisterProperty("NumberLineDeletedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int StampDeletedCount
        {
            get { return GetValue<int>(StampDeletedCountProperty); }
            set { SetValue(StampDeletedCountProperty, value); }
        }

        public static readonly PropertyData StampDeletedCountProperty = RegisterProperty("StampDeletedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int IndividualStampImageDeletedCount
        {
            get { return GetValue<int>(IndividualStampImageDeletedCountProperty); }
            set { SetValue(IndividualStampImageDeletedCountProperty, value); }
        }

        public static readonly PropertyData IndividualStampImageDeletedCountProperty = RegisterProperty("IndividualStampImageDeletedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int StampImageRepresentationDeletedCount
        {
            get { return GetValue<int>(StampImageRepresentationDeletedCountProperty); }
            set { SetValue(StampImageRepresentationDeletedCountProperty, value); }
        }

        public static readonly PropertyData StampImageRepresentationDeletedCountProperty = RegisterProperty("StampImageRepresentationDeletedCount", typeof(int), 0);

        #endregion // Whole Page Characteristics

        #region Left Side

        #region Array

        /// <summary>SUMMARY</summary>
        public int LeftArrayCreatedCount
        {
            get { return GetValue<int>(LeftArrayCreatedCountProperty); }
            set { SetValue(LeftArrayCreatedCountProperty, value); }
        }

        public static readonly PropertyData LeftArrayCreatedCountProperty = RegisterProperty("LeftArrayCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int LeftArrayCutCount
        {
            get { return GetValue<int>(LeftArrayCutCountProperty); }
            set { SetValue(LeftArrayCutCountProperty, value); }
        }

        public static readonly PropertyData LeftArrayCutCountProperty = RegisterProperty("LeftArrayCutCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int LeftArraySnapCount
        {
            get { return GetValue<int>(LeftArraySnapCountProperty); }
            set { SetValue(LeftArraySnapCountProperty, value); }
        }

        public static readonly PropertyData LeftArraySnapCountProperty = RegisterProperty("LeftArraySnapCount", typeof(int), 0);

        /// <summary>Ink Divide count.</summary>
        public int LeftArrayDivideCount
        {
            get { return GetValue<int>(LeftArrayDivideCountProperty); }
            set { SetValue(LeftArrayDivideCountProperty, value); }
        }

        public static readonly PropertyData LeftArrayDivideCountProperty = RegisterProperty("LeftArrayDivideCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int LeftArraySkipCount
        {
            get { return GetValue<int>(LeftArraySkipCountProperty); }
            set { SetValue(LeftArraySkipCountProperty, value); }
        }

        public static readonly PropertyData LeftArraySkipCountProperty = RegisterProperty("LeftArraySkipCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public List<string> LeftArraySkipCountingCorretness
        {
            get { return GetValue<List<string>>(LeftArraySkipCountingCorretnessProperty); }
            set { SetValue(LeftArraySkipCountingCorretnessProperty, value); }
        }

        public static readonly PropertyData LeftArraySkipCountingCorretnessProperty = RegisterProperty("LeftArraySkipCountingCorretness", typeof(List<string>), () => new List<string>());
        
        #endregion // Array

        #region Number Line

        /// <summary>SUMMARY</summary>
        public int LeftNumberLineUsedCount
        {
            get { return GetValue<int>(LeftNumberLineUsedCountProperty); }
            set { SetValue(LeftNumberLineUsedCountProperty, value); }
        }

        public static readonly PropertyData LeftNumberLineUsedCountProperty = RegisterProperty("LeftNumberLineUsedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string LeftNLJE
        {
            get { return GetValue<string>(LeftNLJEProperty); }
            set { SetValue(LeftNLJEProperty, value); }
        }

        public static readonly PropertyData LeftNLJEProperty = RegisterProperty("LeftNLJE", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string LeftNumberLineSwitched
        {
            get { return GetValue<string>(LeftNumberLineSwitchedProperty); }
            set { SetValue(LeftNumberLineSwitchedProperty, value); }
        }

        public static readonly PropertyData LeftNumberLineSwitchedProperty = RegisterProperty("LeftNumberLineSwitched", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string LeftNumberLineBlank
        {
            get { return GetValue<string>(LeftNumberLineBlankProperty); }
            set { SetValue(LeftNumberLineBlankProperty, value); }
        }

        public static readonly PropertyData LeftNumberLineBlankProperty = RegisterProperty("LeftNumberLineBlank", typeof(string), string.Empty);

        #endregion // Number Line

        #region Stamps

        /// <summary>SUMMARY</summary>
        public int LeftStampCreatedCount
        {
            get { return GetValue<int>(LeftStampCreatedCountProperty); }
            set { SetValue(LeftStampCreatedCountProperty, value); }
        }

        public static readonly PropertyData LeftStampCreatedCountProperty = RegisterProperty("LeftStampCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int LeftStampImagesCreatedCount
        {
            get { return GetValue<int>(LeftStampImagesCreatedCountProperty); }
            set { SetValue(LeftStampImagesCreatedCountProperty, value); }
        }

        public static readonly PropertyData LeftStampImagesCreatedCountProperty = RegisterProperty("LeftStampImagesCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string LeftStampImagesSwitched
        {
            get { return GetValue<string>(LeftStampImagesSwitchedProperty); }
            set { SetValue(LeftStampImagesSwitchedProperty, value); }
        }

        public static readonly PropertyData LeftStampImagesSwitchedProperty = RegisterProperty("LeftStampImagesSwitched", typeof(string), string.Empty);

        #endregion // Stamps

        /// <summary>SUMMARY</summary>
        public List<string> LeftRepresentationsAndCorrectness
        {
            get { return GetValue<List<string>>(LeftRepresentationsAndCorrectnessProperty); }
            set { SetValue(LeftRepresentationsAndCorrectnessProperty, value); }
        }

        public static readonly PropertyData LeftRepresentationsAndCorrectnessProperty = RegisterProperty("LeftRepresentationsAndCorrectness", typeof(List<string>), () => new List<string>());

        /// <summary>SUMMARY</summary>
        public string IsLeftMR
        {
            get { return GetValue<string>(IsLeftMRProperty); }
            set { SetValue(IsLeftMRProperty, value); }
        }

        public static readonly PropertyData IsLeftMRProperty = RegisterProperty("IsLeftMR", typeof(string), string.Empty);

        #endregion // Left Side

        #region Right Side

        #region Array

        /// <summary>SUMMARY</summary>
        public int RightArrayCreatedCount
        {
            get { return GetValue<int>(RightArrayCreatedCountProperty); }
            set { SetValue(RightArrayCreatedCountProperty, value); }
        }

        public static readonly PropertyData RightArrayCreatedCountProperty = RegisterProperty("RightArrayCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int RightArrayCutCount
        {
            get { return GetValue<int>(RightArrayCutCountProperty); }
            set { SetValue(RightArrayCutCountProperty, value); }
        }

        public static readonly PropertyData RightArrayCutCountProperty = RegisterProperty("RightArrayCutCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int RightArraySnapCount
        {
            get { return GetValue<int>(RightArraySnapCountProperty); }
            set { SetValue(RightArraySnapCountProperty, value); }
        }

        public static readonly PropertyData RightArraySnapCountProperty = RegisterProperty("RightArraySnapCount", typeof(int), 0);

        /// <summary>Ink Divide count.</summary>
        public int RightArrayDivideCount
        {
            get { return GetValue<int>(RightArrayDivideCountProperty); }
            set { SetValue(RightArrayDivideCountProperty, value); }
        }

        public static readonly PropertyData RightArrayDivideCountProperty = RegisterProperty("RightArrayDivideCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int RightArraySkipCount
        {
            get { return GetValue<int>(RightArraySkipCountProperty); }
            set { SetValue(RightArraySkipCountProperty, value); }
        }

        public static readonly PropertyData RightArraySkipCountProperty = RegisterProperty("RightArraySkipCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public List<string> RightArraySkipCountingCorretness
        {
            get { return GetValue<List<string>>(RightArraySkipCountingCorretnessProperty); }
            set { SetValue(RightArraySkipCountingCorretnessProperty, value); }
        }

        public static readonly PropertyData RightArraySkipCountingCorretnessProperty = RegisterProperty("RightArraySkipCountingCorretness", typeof(List<string>), () => new List<string>());

        #endregion // Array

        #region Number Line

        /// <summary>SUMMARY</summary>
        public int RightNumberLineUsedCount
        {
            get { return GetValue<int>(RightNumberLineUsedCountProperty); }
            set { SetValue(RightNumberLineUsedCountProperty, value); }
        }

        public static readonly PropertyData RightNumberLineUsedCountProperty = RegisterProperty("RightNumberLineUsedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string RightNLJE
        {
            get { return GetValue<string>(RightNLJEProperty); }
            set { SetValue(RightNLJEProperty, value); }
        }

        public static readonly PropertyData RightNLJEProperty = RegisterProperty("RightNLJE", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string RightNumberLineSwitched
        {
            get { return GetValue<string>(RightNumberLineSwitchedProperty); }
            set { SetValue(RightNumberLineSwitchedProperty, value); }
        }

        public static readonly PropertyData RightNumberLineSwitchedProperty = RegisterProperty("RightNumberLineSwitched", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string RightNumberLineBlank
        {
            get { return GetValue<string>(RightNumberLineBlankProperty); }
            set { SetValue(RightNumberLineBlankProperty, value); }
        }

        public static readonly PropertyData RightNumberLineBlankProperty = RegisterProperty("RightNumberLineBlank", typeof(string), string.Empty);

        #endregion // Number Line

        #region Stamps

        /// <summary>SUMMARY</summary>
        public int RightStampCreatedCount
        {
            get { return GetValue<int>(RightStampCreatedCountProperty); }
            set { SetValue(RightStampCreatedCountProperty, value); }
        }

        public static readonly PropertyData RightStampCreatedCountProperty = RegisterProperty("RightStampCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int RightStampImagesCreatedCount
        {
            get { return GetValue<int>(RightStampImagesCreatedCountProperty); }
            set { SetValue(RightStampImagesCreatedCountProperty, value); }
        }

        public static readonly PropertyData RightStampImagesCreatedCountProperty = RegisterProperty("RightStampImagesCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string RightStampImagesSwitched
        {
            get { return GetValue<string>(RightStampImagesSwitchedProperty); }
            set { SetValue(RightStampImagesSwitchedProperty, value); }
        }

        public static readonly PropertyData RightStampImagesSwitchedProperty = RegisterProperty("RightStampImagesSwitched", typeof(string), string.Empty);

        #endregion // Stamps

        /// <summary>SUMMARY</summary>
        public List<string> RightRepresentationsAndCorrectness
        {
            get { return GetValue<List<string>>(RightRepresentationsAndCorrectnessProperty); }
            set { SetValue(RightRepresentationsAndCorrectnessProperty, value); }
        }

        public static readonly PropertyData RightRepresentationsAndCorrectnessProperty = RegisterProperty("RightRepresentationsAndCorrectness", typeof(List<string>), () => new List<string>());

        /// <summary>SUMMARY</summary>
        public string IsRightMR
        {
            get { return GetValue<string>(IsRightMRProperty); }
            set { SetValue(IsRightMRProperty, value); }
        }

        public static readonly PropertyData IsRightMRProperty = RegisterProperty("IsRightMR", typeof(string), string.Empty);

        #endregion // Right Side

        #region Alternative Side

        #region Array

        /// <summary>SUMMARY</summary>
        public int AlternativeArrayCreatedCount
        {
            get { return GetValue<int>(AlternativeArrayCreatedCountProperty); }
            set { SetValue(AlternativeArrayCreatedCountProperty, value); }
        }

        public static readonly PropertyData AlternativeArrayCreatedCountProperty = RegisterProperty("AlternativeArrayCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int AlternativeArrayCutCount
        {
            get { return GetValue<int>(AlternativeArrayCutCountProperty); }
            set { SetValue(AlternativeArrayCutCountProperty, value); }
        }

        public static readonly PropertyData AlternativeArrayCutCountProperty = RegisterProperty("AlternativeArrayCutCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int AlternativeArraySnapCount
        {
            get { return GetValue<int>(AlternativeArraySnapCountProperty); }
            set { SetValue(AlternativeArraySnapCountProperty, value); }
        }

        public static readonly PropertyData AlternativeArraySnapCountProperty = RegisterProperty("AlternativeArraySnapCount", typeof(int), 0);

        /// <summary>Ink Divide count.</summary>
        public int AlternativeArrayDivideCount
        {
            get { return GetValue<int>(AlternativeArrayDivideCountProperty); }
            set { SetValue(AlternativeArrayDivideCountProperty, value); }
        }

        public static readonly PropertyData AlternativeArrayDivideCountProperty = RegisterProperty("AlternativeArrayDivideCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int AlternativeArraySkipCount
        {
            get { return GetValue<int>(AlternativeArraySkipCountProperty); }
            set { SetValue(AlternativeArraySkipCountProperty, value); }
        }

        public static readonly PropertyData AlternativeArraySkipCountProperty = RegisterProperty("AlternativeArraySkipCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public List<string> AlternativeArraySkipCountingCorretness
        {
            get { return GetValue<List<string>>(AlternativeArraySkipCountingCorretnessProperty); }
            set { SetValue(AlternativeArraySkipCountingCorretnessProperty, value); }
        }

        public static readonly PropertyData AlternativeArraySkipCountingCorretnessProperty = RegisterProperty("AlternativeArraySkipCountingCorretness", typeof(List<string>), () => new List<string>());

        #endregion // Array

        #region Number Line

        /// <summary>SUMMARY</summary>
        public int AlternativeNumberLineUsedCount
        {
            get { return GetValue<int>(AlternativeNumberLineUsedCountProperty); }
            set { SetValue(AlternativeNumberLineUsedCountProperty, value); }
        }

        public static readonly PropertyData AlternativeNumberLineUsedCountProperty = RegisterProperty("AlternativeNumberLineUsedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string AlternativeNLJE
        {
            get { return GetValue<string>(AlternativeNLJEProperty); }
            set { SetValue(AlternativeNLJEProperty, value); }
        }

        public static readonly PropertyData AlternativeNLJEProperty = RegisterProperty("AlternativeNLJE", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string AlternativeNumberLineBlank
        {
            get { return GetValue<string>(AlternativeNumberLineBlankProperty); }
            set { SetValue(AlternativeNumberLineBlankProperty, value); }
        }

        public static readonly PropertyData AlternativeNumberLineBlankProperty = RegisterProperty("AlternativeNumberLineBlank", typeof(string), string.Empty);

        #endregion // Number Line

        #region Stamps

        /// <summary>SUMMARY</summary>
        public int AlternativeStampCreatedCount
        {
            get { return GetValue<int>(AlternativeStampCreatedCountProperty); }
            set { SetValue(AlternativeStampCreatedCountProperty, value); }
        }

        public static readonly PropertyData AlternativeStampCreatedCountProperty = RegisterProperty("AlternativeStampCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int AlternativeStampImagesCreatedCount
        {
            get { return GetValue<int>(AlternativeStampImagesCreatedCountProperty); }
            set { SetValue(AlternativeStampImagesCreatedCountProperty, value); }
        }

        public static readonly PropertyData AlternativeStampImagesCreatedCountProperty = RegisterProperty("AlternativeStampImagesCreatedCount", typeof(int), 0);

        #endregion // Stamps

        /// <summary>SUMMARY</summary>
        public List<string> AlternativeRepresentationsAndCorrectness
        {
            get { return GetValue<List<string>>(AlternativeRepresentationsAndCorrectnessProperty); }
            set { SetValue(AlternativeRepresentationsAndCorrectnessProperty, value); }
        }

        public static readonly PropertyData AlternativeRepresentationsAndCorrectnessProperty = RegisterProperty("AlternativeRepresentationsAndCorrectness", typeof(List<string>), () => new List<string>());

        /// <summary>SUMMARY</summary>
        public string IsAlternativeMR
        {
            get { return GetValue<string>(IsAlternativeMRProperty); }
            set { SetValue(IsAlternativeMRProperty, value); }
        }

        public static readonly PropertyData IsAlternativeMRProperty = RegisterProperty("IsAlternativeMR", typeof(string), string.Empty);

        #endregion // Alternative Side

        #region Unmatched Side

        #region Array

        /// <summary>SUMMARY</summary>
        public int UnmatchedArrayCreatedCount
        {
            get { return GetValue<int>(UnmatchedArrayCreatedCountProperty); }
            set { SetValue(UnmatchedArrayCreatedCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedArrayCreatedCountProperty = RegisterProperty("UnmatchedArrayCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int UnmatchedArrayCutCount
        {
            get { return GetValue<int>(UnmatchedArrayCutCountProperty); }
            set { SetValue(UnmatchedArrayCutCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedArrayCutCountProperty = RegisterProperty("UnmatchedArrayCutCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int UnmatchedArraySnapCount
        {
            get { return GetValue<int>(UnmatchedArraySnapCountProperty); }
            set { SetValue(UnmatchedArraySnapCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedArraySnapCountProperty = RegisterProperty("UnmatchedArraySnapCount", typeof(int), 0);

        /// <summary>Ink Divide count.</summary>
        public int UnmatchedArrayDivideCount
        {
            get { return GetValue<int>(UnmatchedArrayDivideCountProperty); }
            set { SetValue(UnmatchedArrayDivideCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedArrayDivideCountProperty = RegisterProperty("UnmatchedArrayDivideCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int UnmatchedArraySkipCount
        {
            get { return GetValue<int>(UnmatchedArraySkipCountProperty); }
            set { SetValue(UnmatchedArraySkipCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedArraySkipCountProperty = RegisterProperty("UnmatchedArraySkipCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public List<string> UnmatchedArraySkipCountingCorretness
        {
            get { return GetValue<List<string>>(UnmatchedArraySkipCountingCorretnessProperty); }
            set { SetValue(UnmatchedArraySkipCountingCorretnessProperty, value); }
        }

        public static readonly PropertyData UnmatchedArraySkipCountingCorretnessProperty = RegisterProperty("UnmatchedArraySkipCountingCorretness", typeof(List<string>), () => new List<string>());

        #endregion // Array

        #region Number Line

        /// <summary>SUMMARY</summary>
        public int UnmatchedNumberLineUsedCount
        {
            get { return GetValue<int>(UnmatchedNumberLineUsedCountProperty); }
            set { SetValue(UnmatchedNumberLineUsedCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedNumberLineUsedCountProperty = RegisterProperty("UnmatchedNumberLineUsedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public string UnmatchedNLJE
        {
            get { return GetValue<string>(UnmatchedNLJEProperty); }
            set { SetValue(UnmatchedNLJEProperty, value); }
        }

        public static readonly PropertyData UnmatchedNLJEProperty = RegisterProperty("UnmatchedNLJE", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string UnmatchedNumberLineBlank
        {
            get { return GetValue<string>(UnmatchedNumberLineBlankProperty); }
            set { SetValue(UnmatchedNumberLineBlankProperty, value); }
        }

        public static readonly PropertyData UnmatchedNumberLineBlankProperty = RegisterProperty("UnmatchedNumberLineBlank", typeof(string), string.Empty);

        #endregion // Number Line

        #region Stamps

        /// <summary>SUMMARY</summary>
        public int UnmatchedStampCreatedCount
        {
            get { return GetValue<int>(UnmatchedStampCreatedCountProperty); }
            set { SetValue(UnmatchedStampCreatedCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedStampCreatedCountProperty = RegisterProperty("UnmatchedStampCreatedCount", typeof(int), 0);

        /// <summary>SUMMARY</summary>
        public int UnmatchedStampImagesCreatedCount
        {
            get { return GetValue<int>(UnmatchedStampImagesCreatedCountProperty); }
            set { SetValue(UnmatchedStampImagesCreatedCountProperty, value); }
        }

        public static readonly PropertyData UnmatchedStampImagesCreatedCountProperty = RegisterProperty("UnmatchedStampImagesCreatedCount", typeof(int), 0);

        #endregion // Stamps

        /// <summary>SUMMARY</summary>
        public List<string> UnmatchedRepresentationsAndCorrectness
        {
            get { return GetValue<List<string>>(UnmatchedRepresentationsAndCorrectnessProperty); }
            set { SetValue(UnmatchedRepresentationsAndCorrectnessProperty, value); }
        }

        public static readonly PropertyData UnmatchedRepresentationsAndCorrectnessProperty = RegisterProperty("UnmatchedRepresentationsAndCorrectness", typeof(List<string>), () => new List<string>());

        /// <summary>SUMMARY</summary>
        public string IsUnmatchedMR
        {
            get { return GetValue<string>(IsUnmatchedMRProperty); }
            set { SetValue(IsUnmatchedMRProperty, value); }
        }

        public static readonly PropertyData IsUnmatchedMRProperty = RegisterProperty("IsUnmatchedMR", typeof(string), string.Empty);

        #endregion // Unmatched Side

        #region Whole Page Analysis

        /// <summary>SUMMARY</summary>
        public string NLJE
        {
            get { return GetValue<string>(NLJEProperty); }
            set { SetValue(NLJEProperty, value); }
        }

        public static readonly PropertyData NLJEProperty = RegisterProperty("NLJE", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsMR2STEP
        {
            get { return GetValue<string>(IsMR2STEPProperty); }
            set { SetValue(IsMR2STEPProperty, value); }
        }

        public static readonly PropertyData IsMR2STEPProperty = RegisterProperty("IsMR2STEP", typeof(string), string.Empty);

        /// <summary>Correctness of the Fill-In/Multiple Choice answer.</summary>
        public string FinalAnswerCorrectness
        {
            get { return GetValue<string>(FinalAnswerCorrectnessProperty); }
            set { SetValue(FinalAnswerCorrectnessProperty, value); }
        }

        public static readonly PropertyData FinalAnswerCorrectnessProperty = RegisterProperty("FinalAnswerCorrectness", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsABR
        {
            get { return GetValue<string>(IsABRProperty); }
            set { SetValue(IsABRProperty, value); }
        }

        public static readonly PropertyData IsABRProperty = RegisterProperty("IsABR", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsRAA
        {
            get { return GetValue<string>(IsRAAProperty); }
            set { SetValue(IsRAAProperty, value); }
        }

        public static readonly PropertyData IsRAAProperty = RegisterProperty("IsRAA", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public List<string> AnswersChangedAfterRepresentation
        {
            get { return GetValue<List<string>>(AnswersChangedAfterRepresentationProperty); }
            set { SetValue(AnswersChangedAfterRepresentationProperty, value); }
        }

        public static readonly PropertyData AnswersChangedAfterRepresentationProperty = RegisterProperty("AnswersChangedAfterRepresentation", typeof(List<string>), () => new List<string>());

        /// <summary>Total number of the distinct ink colors used on the page.</summary>
        public int InkColorsUsedCount
        {
            get { return GetValue<int>(InkColorsUsedCountProperty); }
            set { SetValue(InkColorsUsedCountProperty, value); }
        }

        public static readonly PropertyData InkColorsUsedCountProperty = RegisterProperty("InkColorsUsedCount", typeof(int), 0);

        #endregion // Whole Page Analysis

        #region Total History

        /// <summary>SUMMARY</summary>
        public List<string> FinalSemanticEvents
        {
            get { return GetValue<List<string>>(FinalSemanticEventsProperty); }
            set { SetValue(FinalSemanticEventsProperty, value); }
        }

        public static readonly PropertyData FinalSemanticEventsProperty = RegisterProperty("FinalSemanticEvents", typeof(List<string>), () => new List<string>());

        #endregion // Total History

        #endregion // Properties

        #region Static Properties

        //public Dictionary<string,int> 

        #endregion // Static Properties

        #region Methods

        public string BuildEntryLine()
        {
            var cellContents = new List<string>();

            // Page Identification
            cellContents.Add(OwnerName);
            cellContents.Add(PageNumber.ToString());
            cellContents.Add(SubmissionTime);

            // Problem Characteristics
            cellContents.Add(ProblemType);
            cellContents.Add(WordType);
            cellContents.Add(LeftSideOperation);
            cellContents.Add(RightSideOperation);
            cellContents.Add(DivisionType);
            cellContents.Add(IsMultipleChoiceBoxOnPage);
            cellContents.Add(DifficultyLevel);
            cellContents.Add(PageDefinitionEquation);
            cellContents.Add(RequiredRepresentations);
            cellContents.Add(string.Join(", ", SpecialInterestGroups));

            // Whole Page Characteristics
            cellContents.Add(IsInkOnly);
            cellContents.Add(IsBlank);
            cellContents.Add(ArrayDeletedCount.ToString());
            cellContents.Add(NumberLineCreatedCount.ToString());
            cellContents.Add(NumberLineDeletedCount.ToString());
            cellContents.Add(StampDeletedCount.ToString());
            cellContents.Add(IndividualStampImageDeletedCount.ToString());
            cellContents.Add(StampImageRepresentationDeletedCount.ToString());

            // Left Side
            cellContents.Add(LeftArrayCreatedCount.ToString());
            cellContents.Add(LeftArrayCutCount.ToString());
            cellContents.Add(LeftArraySnapCount.ToString());
            cellContents.Add(LeftArrayDivideCount.ToString());
            cellContents.Add(LeftArraySkipCount.ToString());
            cellContents.Add(string.Join(", ", LeftArraySkipCountingCorretness));
            cellContents.Add(LeftNumberLineUsedCount.ToString());
            cellContents.Add(LeftNumberLineSwitched);
            cellContents.Add(LeftNumberLineBlank);
            cellContents.Add(LeftStampCreatedCount.ToString());
            cellContents.Add(LeftStampImagesCreatedCount.ToString());
            cellContents.Add(LeftStampImagesSwitched);
            cellContents.Add(string.Join(", ", LeftRepresentationsAndCorrectness));
            cellContents.Add(IsLeftMR);

            // Right Side
            cellContents.Add(RightArrayCreatedCount.ToString());
            cellContents.Add(RightArrayCutCount.ToString());
            cellContents.Add(RightArraySnapCount.ToString());
            cellContents.Add(RightArrayDivideCount.ToString());
            cellContents.Add(RightArraySkipCount.ToString());
            cellContents.Add(string.Join(", ", RightArraySkipCountingCorretness));
            cellContents.Add(RightNumberLineUsedCount.ToString());
            cellContents.Add(RightNumberLineSwitched);
            cellContents.Add(RightNumberLineBlank);
            cellContents.Add(RightStampCreatedCount.ToString());
            cellContents.Add(RightStampImagesCreatedCount.ToString());
            cellContents.Add(RightStampImagesSwitched);
            cellContents.Add(string.Join(", ", RightRepresentationsAndCorrectness));
            cellContents.Add(IsRightMR);

            // Alternative Side
            cellContents.Add(AlternativeArrayCreatedCount.ToString());
            cellContents.Add(AlternativeArrayCutCount.ToString());
            cellContents.Add(AlternativeArraySnapCount.ToString());
            cellContents.Add(AlternativeArrayDivideCount.ToString());
            cellContents.Add(AlternativeArraySkipCount.ToString());
            cellContents.Add(string.Join(", ", AlternativeArraySkipCountingCorretness));
            cellContents.Add(AlternativeNumberLineUsedCount.ToString());
            cellContents.Add(AlternativeNumberLineBlank);
            cellContents.Add(AlternativeStampCreatedCount.ToString());
            cellContents.Add(AlternativeStampImagesCreatedCount.ToString());
            cellContents.Add(string.Join(", ", AlternativeRepresentationsAndCorrectness));
            cellContents.Add(IsAlternativeMR);

            // Unmatched Side
            cellContents.Add(UnmatchedArrayCreatedCount.ToString());
            cellContents.Add(UnmatchedArrayCutCount.ToString());
            cellContents.Add(UnmatchedArraySnapCount.ToString());
            cellContents.Add(UnmatchedArrayDivideCount.ToString());
            cellContents.Add(UnmatchedArraySkipCount.ToString());
            cellContents.Add(string.Join(", ", UnmatchedArraySkipCountingCorretness));
            cellContents.Add(UnmatchedNumberLineUsedCount.ToString());
            cellContents.Add(UnmatchedNumberLineBlank);
            cellContents.Add(UnmatchedStampCreatedCount.ToString());
            cellContents.Add(UnmatchedStampImagesCreatedCount.ToString());
            cellContents.Add(string.Join(", ", UnmatchedRepresentationsAndCorrectness));
            cellContents.Add(IsUnmatchedMR);

            // Whole Page Analysis
            cellContents.Add(NLJE);
            cellContents.Add(IsMR2STEP);
            cellContents.Add(FinalAnswerCorrectness);
            cellContents.Add(IsABR);
            cellContents.Add(IsRAA);
            cellContents.Add(string.Join(", ", AnswersChangedAfterRepresentation));
            cellContents.Add(InkColorsUsedCount.ToString());

            // Total History
            cellContents.Add(string.Join(", ", FinalSemanticEvents));

            return string.Join("\t", cellContents);
        }

        #endregion // Methods

        #region Static Methods

        public static string BuildHeaderEntryLine()
        {
            var cellContents = new List<string>();

            // Page Identification
            cellContents.Add("Student Name");
            cellContents.Add("Page Number");
            cellContents.Add("Submission Time");

            // Problem Characteristics
            cellContents.Add("Problem Type");
            cellContents.Add("Word/Non-Word");
            cellContents.Add("LS OP");
            cellContents.Add("RS OP");
            cellContents.Add("Division Type");
            cellContents.Add("MC");
            cellContents.Add("Level");
            cellContents.Add("Equation");
            cellContents.Add("Required Rep");
            cellContents.Add("SIG");

            // Whole Page Characteristics
            cellContents.Add("INK Only");
            cellContents.Add("Blank");
            cellContents.Add("ARR Deleted");
            cellContents.Add("NL Created");
            cellContents.Add("NL Deleted");
            cellContents.Add("STA Deleted");
            cellContents.Add("STA IMAGES Deleted");
            cellContents.Add("ALL STA IMAGES on page Deleted");

            // Left Side
            cellContents.Add("Left ARR created");
            cellContents.Add("Left ARR cut");
            cellContents.Add("Left ARR snap");
            cellContents.Add("Left ARR divide");
            cellContents.Add("Left ARR skip");
            cellContents.Add("Left ARR skip tags");
            cellContents.Add("Left NL used");
            cellContents.Add("Left NL switched");
            cellContents.Add("Left NL blank");
            cellContents.Add("Left STA created");
            cellContents.Add("Left STA IMAGES");
            cellContents.Add("Left STA switched");
            cellContents.Add("Left REP Correctness");
            cellContents.Add("Left MR");

            // Right Side
            cellContents.Add("Right ARR created");
            cellContents.Add("Right ARR cut");
            cellContents.Add("Right ARR snap");
            cellContents.Add("Right ARR divide");
            cellContents.Add("Right ARR skip");
            cellContents.Add("Right ARR skip tags");
            cellContents.Add("Right NL used");
            cellContents.Add("Right NL switched");
            cellContents.Add("Right NL blank");
            cellContents.Add("Right STA created");
            cellContents.Add("Right STA IMAGES");
            cellContents.Add("Right STA switched");
            cellContents.Add("Right REP Correctness");
            cellContents.Add("Right MR");

            // Alternative Side
            cellContents.Add("Alternative ARR created");
            cellContents.Add("Alternative ARR cut");
            cellContents.Add("Alternative ARR snap");
            cellContents.Add("Alternative ARR divide");
            cellContents.Add("Alternative ARR skip");
            cellContents.Add("Alternative ARR skip tags");
            cellContents.Add("Alternative NL used");
            cellContents.Add("Alternative NL blank");
            cellContents.Add("Alternative STA created");
            cellContents.Add("Alternative STA IMAGES");
            cellContents.Add("Alternative REP Correctness");
            cellContents.Add("Alternative MR");

            // Unmatched Side
            cellContents.Add("Unmatched ARR created");
            cellContents.Add("Unmatched ARR cut");
            cellContents.Add("Unmatched ARR snap");
            cellContents.Add("Unmatched ARR divide");
            cellContents.Add("Unmatched ARR skip");
            cellContents.Add("Unmatched ARR skip tags");
            cellContents.Add("Unmatched NL used");
            cellContents.Add("Unmatched NL blank");
            cellContents.Add("Unmatched STA created");
            cellContents.Add("Unmatched STA IMAGES");
            cellContents.Add("Unmatched REP Correctness");
            cellContents.Add("Unmatched MR");

            // Whole Page Analysis
            cellContents.Add("NLJE");
            cellContents.Add("MR2STEP");
            cellContents.Add("Final ANS");
            cellContents.Add("ABR");
            cellContents.Add("RAA");
            cellContents.Add("ANS Changed");
            cellContents.Add("Colors");

            // Total History
            cellContents.Add("History");

            return string.Join("\t", cellContents);
        }

        #endregion // Static Methods
    }
}

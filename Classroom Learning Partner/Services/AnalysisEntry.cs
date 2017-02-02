using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>If 1-Part problem is a multiplication problem that uses groups.</summary>
        public string IsMultiplicationProblemUsingGroups
        {
            get { return GetValue<string>(IsMultiplicationProblemUsingGroupsProperty); }
            set { SetValue(IsMultiplicationProblemUsingGroupsProperty, value); }
        }

        public static readonly PropertyData IsMultiplicationProblemUsingGroupsProperty = RegisterProperty("IsMultiplicationProblemUsingGroups", typeof(string), string.Empty);

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




        #endregion // Left Side


        #region Whole Page Analysis

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

        /// <summary>Overall correctness summary of the page.</summary>
        public string CorrectnessSummary
        {
            get { return GetValue<string>(CorrectnessSummaryProperty); }
            set { SetValue(CorrectnessSummaryProperty, value); }
        }

        public static readonly PropertyData CorrectnessSummaryProperty = RegisterProperty("CorrectnessSummary", typeof(string), string.Empty);

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
            cellContents.Add(IsMultiplicationProblemUsingGroups);
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
            cellContents.Add(LeftNumberLineUsedCount.ToString());
            cellContents.Add(LeftNLJE);
            cellContents.Add(LeftNumberLineSwitched);
            cellContents.Add(LeftNumberLineBlank);

            // Whole Page Analysis
            cellContents.Add(IsMR2STEP);
            cellContents.Add(FinalAnswerCorrectness);
            cellContents.Add(CorrectnessSummary);
            cellContents.Add(IsABR);
            cellContents.Add(IsRAA);
            cellContents.Add("ANS Changed");
            cellContents.Add(InkColorsUsedCount.ToString());

            // Total History
            cellContents.Add(string.Join(", ", FinalSemanticEvents));

            return string.Join("\t", cellContents);
        }

        #endregion // Methods

        #region Static Methods

        public string BuildHeaderEntryLine()
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
            cellContents.Add("Groups");
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
            cellContents.Add("Left ARR Created");
            cellContents.Add("Left ARR Cut");
            cellContents.Add("Left ARR Snap");
            cellContents.Add("Left ARR Divide");
            cellContents.Add("Left ARR Skip");
            cellContents.Add("Groups");
            cellContents.Add("Required Rep");
            cellContents.Add("SIG");
            cellContents.Add("RS OP");
            cellContents.Add("Division Type");
            cellContents.Add("MC");
            cellContents.Add("Level");
            cellContents.Add("Equation");
            cellContents.Add("Groups");
            cellContents.Add("Required Rep");
            cellContents.Add("SIG");

            // Whole Page Analysis
            cellContents.Add("MR2STEP");
            cellContents.Add("Final ANS");
            cellContents.Add("Correctness Summary");
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

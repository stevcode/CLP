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
        public const string UNSUBMITTED = "UNSUBMITTED";

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

        /// <summary>SUMMARY</summary>
        public string SubmissionTime
        {
            get { return GetValue<string>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(string), string.Empty);

        #endregion // Page Identification

        #region Problem Characteristics

        /// <summary>SUMMARY</summary>
        public string ProblemType
        {
            get { return GetValue<string>(ProblemTypeProperty); }
            set { SetValue(ProblemTypeProperty, value); }
        }

        public static readonly PropertyData ProblemTypeProperty = RegisterProperty("ProblemType", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string WordType
        {
            get { return GetValue<string>(WordTypeProperty); }
            set { SetValue(WordTypeProperty, value); }
        }

        public static readonly PropertyData WordTypeProperty = RegisterProperty("WordType", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string LeftSideOperation
        {
            get { return GetValue<string>(LeftSideOperationProperty); }
            set { SetValue(LeftSideOperationProperty, value); }
        }

        public static readonly PropertyData LeftSideOperationProperty = RegisterProperty("LeftSideOperation", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string RightSideOperation
        {
            get { return GetValue<string>(RightSideOperationProperty); }
            set { SetValue(RightSideOperationProperty, value); }
        }

        public static readonly PropertyData RightSideOperationProperty = RegisterProperty("RightSideOperation", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string DivisionType
        {
            get { return GetValue<string>(DivisionTypeProperty); }
            set { SetValue(DivisionTypeProperty, value); }
        }

        public static readonly PropertyData DivisionTypeProperty = RegisterProperty("DivisionType", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsMultipleChoiceBoxOnPage
        {
            get { return GetValue<string>(IsMultipleChoiceBoxOnPageProperty); }
            set { SetValue(IsMultipleChoiceBoxOnPageProperty, value); }
        }

        public static readonly PropertyData IsMultipleChoiceBoxOnPageProperty = RegisterProperty("IsMultipleChoiceBoxOnPage", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string DifficultyLevel
        {
            get { return GetValue<string>(DifficultyLevelProperty); }
            set { SetValue(DifficultyLevelProperty, value); }
        }

        public static readonly PropertyData DifficultyLevelProperty = RegisterProperty("DifficultyLevel", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string PageDefinitionEquation
        {
            get { return GetValue<string>(PageDefinitionEquationProperty); }
            set { SetValue(PageDefinitionEquationProperty, value); }
        }

        public static readonly PropertyData PageDefinitionEquationProperty = RegisterProperty("PageDefinitionEquation", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string IsMultiplicationProblemUsingGroups
        {
            get { return GetValue<string>(IsMultiplicationProblemUsingGroupsProperty); }
            set { SetValue(IsMultiplicationProblemUsingGroupsProperty, value); }
        }

        public static readonly PropertyData IsMultiplicationProblemUsingGroupsProperty = RegisterProperty("IsMultiplicationProblemUsingGroups", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public List<string> RequiredRepresentations
        {
            get { return GetValue<List<string>>(RequiredRepresentationsProperty); }
            set { SetValue(RequiredRepresentationsProperty, value); }
        }

        public static readonly PropertyData RequiredRepresentationsProperty = RegisterProperty("RequiredRepresentations", typeof(List<string>), () => new List<string>());

        /// <summary>SUMMARY</summary>
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
        public int RepresentationsDeletedCount
        {
            get { return GetValue<int>(RepresentationsDeletedCountProperty); }
            set { SetValue(RepresentationsDeletedCountProperty, value); }
        }

        public static readonly PropertyData RepresentationsDeletedCountProperty = RegisterProperty("RepresentationsDeletedCount", typeof(int), 0);

        #endregion // Whole Page Characteristics


        #region Whole Page Analysis

        /// <summary>SUMMARY</summary>
        public string IsMR2STEP
        {
            get { return GetValue<string>(IsMR2STEPProperty); }
            set { SetValue(IsMR2STEPProperty, value); }
        }

        public static readonly PropertyData IsMR2STEPProperty = RegisterProperty("IsMR2STEP", typeof(string), string.Empty);



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
            cellContents.Add(string.Join(", ", RequiredRepresentations));
            cellContents.Add(string.Join(", ", SpecialInterestGroups));

            // Whole Page Characteristics
            cellContents.Add(IsInkOnly);
            cellContents.Add(IsBlank);
            cellContents.Add(RepresentationsDeletedCount.ToString());

            // Whole Page Analysis
            cellContents.Add(IsMR2STEP);
            cellContents.Add("Final ANS");
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
            cellContents.Add("Reps Deleted");

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

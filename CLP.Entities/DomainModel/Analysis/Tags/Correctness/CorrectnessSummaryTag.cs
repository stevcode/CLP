using System;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CorrectnessSummaryTag : AAnalysisTagBase
    {
        #region Constructors

        public CorrectnessSummaryTag() { }

        public CorrectnessSummaryTag(CLPPage parentPage, Origin origin, bool isAutomaticallySet)
            : base(parentPage, origin)
        {
            IsCorrectnessAutomaticallySet = isAutomaticallySet;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Correctness of the final Representations on the page.</summary>
        public Correctness FinalRepresentationCorrectness
        {
            get => GetValue<Correctness>(FinalRepresentationCorrectnessProperty);
            set => SetValue(FinalRepresentationCorrectnessProperty, value);
        }

        public static readonly PropertyData FinalRepresentationCorrectnessProperty = RegisterProperty(nameof(FinalRepresentationCorrectness), typeof(Correctness), Correctness.Unknown);

        /// <summary>Correctness of the final fill-in/multiple choice answer on the page.</summary>
        public Correctness FinalAnswerCorrectness
        {
            get => GetValue<Correctness>(FinalAnswerCorrectnessProperty);
            set => SetValue(FinalAnswerCorrectnessProperty, value);
        }

        public static readonly PropertyData FinalAnswerCorrectnessProperty = RegisterProperty(nameof(FinalAnswerCorrectness), typeof(Correctness), Correctness.Unknown);

        /// <summary>Type of correctness.</summary>
        public Correctness OverallCorrectness
        {
            get => GetValue<Correctness>(OverallCorrectnessProperty);
            set => SetValue(OverallCorrectnessProperty, value);
        }

        public static readonly PropertyData OverallCorrectnessProperty = RegisterProperty(nameof(OverallCorrectness), typeof(Correctness), Correctness.Unknown);

        /// <summary>Signifies the Correctness Tag was set by an analysis routine.</summary>
        public bool IsCorrectnessAutomaticallySet
        {
            get => GetValue<bool>(IsCorrectnessAutomaticallySetProperty);
            set => SetValue(IsCorrectnessAutomaticallySetProperty, value);
        }

        public static readonly PropertyData IsCorrectnessAutomaticallySetProperty = RegisterProperty("IsCorrectnessAutomaticallySet", typeof(bool), false);

        public bool IsCorrectnessManuallySet => Origin == Origin.Teacher;

        #endregion //Properties

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Correctness;

        public override string FormattedName => "Overall Correctness";

        public override string FormattedValue
        {
            get
            {
                var overallCorrectness =
                    $"{OverallCorrectness}, {(IsCorrectnessManuallySet ? "Set by Instructor" : IsCorrectnessAutomaticallySet ? "Set by Analysis" : string.Empty)}";
                var analysisCodes = string.Join("\n", QueryCodes.Select(c => c.FormattedValue));
                var codedSection = QueryCodes.Any() ? $"\nCodes:\n{analysisCodes}" : string.Empty;
                return $"Combined Correctness of Final Representations and Final Answer:\n\t{overallCorrectness}{codedSection}";
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, FinalRepresentationCorrectnessTag finalRepresentationCorrectnessTag, FinalAnswerCorrectnessTag finalAnswerCorrectnessTag)
        {
            if (finalRepresentationCorrectnessTag == null &&
                finalAnswerCorrectnessTag == null)
            {
                return;
            }

            var tag = new CorrectnessSummaryTag(page, Origin.StudentPageGenerated, true)
                      {
                          FinalRepresentationCorrectness =
                              finalRepresentationCorrectnessTag?.FinalRepresentationCorrectness ??
                              Correctness.Unknown,
                          FinalAnswerCorrectness =
                              finalAnswerCorrectnessTag?.FinalAnswerCorrectness ?? Correctness.Unknown
                      };

            var correctness = Correctness.Unknown;
            if (tag.FinalAnswerCorrectness == Correctness.Correct &&
                (tag.FinalRepresentationCorrectness == Correctness.Correct || 
                 tag.FinalRepresentationCorrectness == Correctness.Unanswered))
            {
                correctness = Correctness.Correct;
            }
            else if (tag.FinalAnswerCorrectness == Correctness.Incorrect &&
                     (tag.FinalRepresentationCorrectness == Correctness.Incorrect ||
                      tag.FinalRepresentationCorrectness == Correctness.Unanswered))
            {
                correctness = Correctness.Incorrect;
            }
            else if (tag.FinalAnswerCorrectness == Correctness.PartiallyCorrect ||
                     tag.FinalRepresentationCorrectness == Correctness.PartiallyCorrect ||
                     tag.FinalAnswerCorrectness == Correctness.Correct ||
                     tag.FinalRepresentationCorrectness == Correctness.Correct)
            {
                correctness = Correctness.PartiallyCorrect;
            }

            tag.OverallCorrectness = correctness;

            AnalysisCode.AddOverallCorrectness(tag, Codings.CorrectnessToCodedCorrectness(tag.OverallCorrectness));

            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}
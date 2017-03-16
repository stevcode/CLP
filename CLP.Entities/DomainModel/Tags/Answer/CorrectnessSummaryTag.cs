using System;
using Catel.Data;

namespace CLP.Entities
{
    public enum Correctness
    {
        Correct,
        PartiallyCorrect,
        Incorrect,
        Illegible,
        Unanswered,
        Unknown
    }

    [Serializable]
    public class CorrectnessSummaryTag : ATagBase
    {
        #region Constructors

        public CorrectnessSummaryTag() { }

        public CorrectnessSummaryTag(CLPPage parentPage, Origin origin, Correctness correctness, bool isAutomaticallySet)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            IsCorrectnessAutomaticallySet = isAutomaticallySet;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness), Correctness.Unknown);

        /// <summary>Signifies the Correctness Tag was set by an analysis routine.</summary>
        public bool IsCorrectnessAutomaticallySet
        {
            get { return GetValue<bool>(IsCorrectnessAutomaticallySetProperty); }
            set { SetValue(IsCorrectnessAutomaticallySetProperty, value); }
        }

        public static readonly PropertyData IsCorrectnessAutomaticallySetProperty = RegisterProperty("IsCorrectnessAutomaticallySet", typeof(bool), false);

        public bool IsCorrectnessManuallySet => Origin == Origin.Teacher;

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Answer;

        public override string FormattedName => "Correctness";

        public override string FormattedValue => $"{Correctness}, {(IsCorrectnessManuallySet ? "Set by Instructor" : IsCorrectnessAutomaticallySet ? "Set Automatically" : string.Empty)}";

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, RepresentationCorrectnessTag representationCorrectnessTag, FinalAnswerCorrectnessTag finalAnswerCorrectnessTag)
        {
            if (representationCorrectnessTag == null &&
                finalAnswerCorrectnessTag == null)
            {
                return;
            }

            CorrectnessSummaryTag tag = null;
            if (representationCorrectnessTag == null)
            {
                tag = new CorrectnessSummaryTag(page, Origin.StudentPageGenerated, finalAnswerCorrectnessTag.FinalAnswerCorrectness, true);
            }
            else if (finalAnswerCorrectnessTag == null)
            {
                tag = new CorrectnessSummaryTag(page, Origin.StudentPageGenerated, representationCorrectnessTag.RepresentationCorrectness, true);
            }

            if (tag != null)
            {
                page.AddTag(tag);
                return;
            }

            var representationCorrectness = representationCorrectnessTag.RepresentationCorrectness;
            var finalAnswerCorrectness = finalAnswerCorrectnessTag.FinalAnswerCorrectness;

            var correctness = Correctness.Unknown;
            if (finalAnswerCorrectness == Correctness.Correct &&
                representationCorrectness == Correctness.Correct)
            {
                correctness = Correctness.Correct;
            }
            else if (finalAnswerCorrectness == Correctness.Incorrect &&
                representationCorrectness == Correctness.Incorrect)
            {
                correctness = Correctness.Incorrect;
            }
            else if (finalAnswerCorrectness == Correctness.PartiallyCorrect ||
                     representationCorrectness == Correctness.PartiallyCorrect ||
                     finalAnswerCorrectness == Correctness.Correct ||
                     representationCorrectness == Correctness.Correct)
            {
                correctness = Correctness.PartiallyCorrect;
            }

            tag = new CorrectnessSummaryTag(page, Origin.StudentPageGenerated, correctness, true);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}
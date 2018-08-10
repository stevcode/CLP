using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CorrectnessSummaryTag : ATagBase
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

        /// <summary>Correctness of the final Strategies on the page.</summary>
        public Correctness FinalStrategyCorrectness
        {
            get => GetValue<Correctness>(FinalStrategyCorrectnessProperty);
            set => SetValue(FinalStrategyCorrectnessProperty, value);
        }

        public static readonly PropertyData FinalStrategyCorrectnessProperty = RegisterProperty(nameof(FinalStrategyCorrectness), typeof(Correctness), Correctness.Unknown);

        /// <summary>Correctness of the final fill-in/multiple choice answer on the page.</summary>
        public Correctness FinalAnswerCorrectness
        {
            get => GetValue<Correctness>(FinalAnswerCorrectnessProperty);
            set => SetValue(FinalAnswerCorrectnessProperty, value);
        }

        public static readonly PropertyData FinalAnswerCorrectnessProperty = RegisterProperty(nameof(FinalAnswerCorrectness), typeof(Correctness), Correctness.Unknown);

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get => GetValue<Correctness>(CorrectnessProperty);
            set => SetValue(CorrectnessProperty, value);
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness), Correctness.Unknown);

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

        public override Category Category => TagCategory;

        public override string FormattedName => TagName;

        public override string FormattedValue =>
            $"{Correctness}, {(IsCorrectnessManuallySet ? "Set by Instructor" : IsCorrectnessAutomaticallySet ? "Set Automatically" : string.Empty)}";

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

            //var correctnesses = new List<Correctness> {Final}

            //var correctness = Correctness.Unknown;
            //if (tag.FinalAnswerCorrectness == Correctness.Correct &&
            //    tag.FinalRepresentationCorrectness == Correctness.Correct)
            //{
            //    correctness = Correctness.Correct;
            //}
            //else if (finalAnswerCorrectness == Correctness.Incorrect &&
            //         representationCorrectness == Correctness.Incorrect)
            //{
            //    correctness = Correctness.Incorrect;
            //}
            //else if (finalAnswerCorrectness == Correctness.PartiallyCorrect ||
            //         representationCorrectness == Correctness.PartiallyCorrect ||
            //         finalAnswerCorrectness == Correctness.Correct ||
            //         representationCorrectness == Correctness.Correct)
            //{
            //    correctness = Correctness.PartiallyCorrect;
            //}

            page.AddTag(tag);
        }

        #endregion // Static Methods

        #region Documentation Generation

        public static string TagName => Codings.TAG_NAME_CORRECTNESS_SUMMARY;

        public static Category TagCategory => Category.Correctness;

        public static List<string> PropertyNames => new List<string>();

        public static Dictionary<string, List<string>> PropertiesAndPossibleValues => new Dictionary<string, List<string>>();

        public static void PopulatePropertyNames()
        {
            #region Base

            PropertyNames.Add(Codings.TAG_PROPERTY_NAME_BASE_CATEGORY);
            PropertiesAndPossibleValues.Add(Codings.TAG_PROPERTY_NAME_BASE_CATEGORY, new List<string>());
            PropertiesAndPossibleValues[Codings.TAG_PROPERTY_NAME_BASE_CATEGORY].Add(TagCategory.ToDescription());

            #endregion // Base
        }

        public static string GenerateTagDocumentation()
        {
            var properties = string.Empty;
            foreach (var propertyName in PropertyNames)
            {
                var values = string.Join("\n*\t - ", PropertiesAndPossibleValues[propertyName]);
                properties += $"\n* {propertyName}\n" + $"*\t - {values}\n";
            }

            return $"*****\n" + $"* Tag Name: {TagName}\n\n" + $"* Properties:\n" + $"{properties}" + $"*****\n\n";
        }

        #endregion // Documentation Generation
    }
}
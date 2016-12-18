using System;
using Catel.Data;

namespace CLP.Entities
{
    public enum Correctness
    {
        Correct,
        PartiallyCorrect,
        Incorrect,
        Unknown
    }

    [Serializable]
    public class CorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CorrectnessTag" /> from scratch.</summary>
        public CorrectnessTag() { }

        /// <summary>Initializes <see cref="CorrectnessTag" />.</summary>
        public CorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness, bool isAutomaticallySet)
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

        public bool IsCorrectnessManuallySet
        {
            get { return Origin == Origin.Teacher; }
        }

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.OtherPageInformation;

        public override string FormattedName => "Correctness";

        public override string FormattedValue
        {
            get { return string.Format("{0}, {1}", Correctness, IsCorrectnessManuallySet ? "Set by Instructor" : IsCorrectnessAutomaticallySet ? "Set Automatically" : "Default"); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
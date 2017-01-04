using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
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
            IsSingleValueTag = true;
            Correctness = correctness;
            IsCorrectnessAutomaticallySet = isAutomaticallySet;
        }

        /// <summary>Initializes <see cref="CorrectnessTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof (Correctness));

        /// <summary>Signifies the Correctness Tag was set by an analysis routine.</summary>
        public bool IsCorrectnessAutomaticallySet
        {
            get { return GetValue<bool>(IsCorrectnessAutomaticallySetProperty); }
            set { SetValue(IsCorrectnessAutomaticallySetProperty, value); }
        }

        public static readonly PropertyData IsCorrectnessAutomaticallySetProperty = RegisterProperty("IsCorrectnessAutomaticallySet",
                                                                                                     typeof (bool),
                                                                                                     false);

        public bool IsCorrectnessManuallySet
        {
            get { return Origin == Origin.Teacher; }
        }

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedName
        {
            get { return "Correctness"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}, {1}", Correctness, IsCorrectnessManuallySet ? "Set by Instructor" : IsCorrectnessAutomaticallySet ? "Set Automatically" : "Default"); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
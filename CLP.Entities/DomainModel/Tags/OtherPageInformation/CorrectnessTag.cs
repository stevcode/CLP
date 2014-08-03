﻿using System;
using System.Runtime.Serialization;
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

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> from scratch.
        /// </summary>
        public CorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" />.
        /// </summary>
        public CorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;
            Correctness = correctness;
        }

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Type of correctness.
        /// </summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness));

        public bool IsCorrectnessInterpreted
        {
            get { return Origin != Origin.Teacher; }
        }

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}, {1}", Correctness, IsCorrectnessInterpreted ? "Interpreted" : "Manually Set by Teacher"); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
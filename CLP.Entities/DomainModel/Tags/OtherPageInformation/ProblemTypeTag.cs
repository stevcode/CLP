using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    public enum ProblemTypes
    {
        WordProblem,
        NonWordProblem,
        Division,
        Multiplication,
        Addition,
        Subtraction
    }

    [Serializable]
    public class ProblemTypeTag : ATagBase  // TODO: Implement a way to manually add and/or edit this tag.
    {
        #region Constructors

        /// <summary>Initializes <see cref="ProblemTypeTag" /> from scratch.</summary>
        public ProblemTypeTag() { }

        /// <summary>Initializes <see cref="ProblemTypeTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ProblemTypeTag" /> belongs to.</param>
        /// <param name="origin"></param>
        /// <param name="currentUserID"></param>
        public ProblemTypeTag(CLPPage parentPage, Origin origin, List<ProblemTypes> problemTypes)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            ProblemTypes = problemTypes;
        }

        /// <summary>Initializes <see cref="ProblemTypeTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ProblemTypeTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the <see cref="ProblemTypes" />s on the <see cref="CLPPage" />.</summary>
        public List<ProblemTypes> ProblemTypes
        {
            get { return GetValue<List<ProblemTypes>>(ProblemTypesProperty); }
            set { SetValue(ProblemTypesProperty, value); }
        }

        public static readonly PropertyData ProblemTypesProperty = RegisterProperty("ProblemTypes", typeof (List<ProblemTypes>), () => new List<ProblemTypes>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedName
        {
            get { return "Problem Type"; }
        }

        public override string FormattedValue
        {
            get { return string.Join(", ", ProblemTypes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
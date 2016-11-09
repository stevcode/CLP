﻿using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
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
    public class ProblemTypeTag : ATagBase // TODO: Implement a way to manually add and/or edit this tag.
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
            ProblemTypes = problemTypes;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the <see cref="ProblemTypes" />s on the <see cref="CLPPage" />.</summary>
        public List<ProblemTypes> ProblemTypes
        {
            get { return GetValue<List<ProblemTypes>>(ProblemTypesProperty); }
            set { SetValue(ProblemTypesProperty, value); }
        }

        public static readonly PropertyData ProblemTypesProperty = RegisterProperty("ProblemTypes", typeof(List<ProblemTypes>), () => new List<ProblemTypes>());

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.OtherPageInformation;

        public override string FormattedName => "Problem Type";

        public override string FormattedValue
        {
            get { return string.Join(", ", ProblemTypes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}
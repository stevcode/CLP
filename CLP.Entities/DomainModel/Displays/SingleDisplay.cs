﻿using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class SingleDisplay : ADisplayBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="SingleDisplay" /> from scratch.
        /// </summary>
        public SingleDisplay() { }

        /// <summary>
        /// Initializes <see cref="SingleDisplay" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public SingleDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Methods

        public override void AddPageToDisplay(CLPPage page)
        {
            var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
            PageIDs.Clear();
            PageIDs.Add(pageID);
            Pages.Clear();
            Pages.Add(page);
            CurrentPage = page;
        }

        public override void RemovePageFromDisplay(CLPPage page) { }

        #endregion //Methods
    }
}
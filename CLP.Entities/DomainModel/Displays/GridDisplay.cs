using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class GridDisplay : ADisplayBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="GridDisplay" /> from scratch.
        /// </summary>
        public GridDisplay() { }

        /// <summary>
        /// Initializes <see cref="GridDisplay" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public GridDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Methods

        public override void AddPageToDisplay(CLPPage page)
        {
            var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
            PageIDs.Add(pageID);
            Pages.Add(page);
        }

        public override void RemovePageFromDisplay(CLPPage page)
        {
            PageIDs.Remove(page.ID);
            Pages.Remove(page);
        }

        #endregion //Methods
    }
}
using System;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPMirrorDisplay : ACLPDisplayBase
    {
        #region Constructors
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPMirrorDisplay()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPMirrorDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// The page currently displayed on the MirrorDisplay.
        /// </summary>
        [ExcludeFromSerialization]
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            private set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage), includeInSerialization:false);

        #endregion //Properties

        #region Methods

        public override void AddPageToDisplay(ICLPPage page)
        {
            DisplayPageIDs.Clear();
            var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;
            DisplayPageIDs.Add(pageID);
            CurrentPage = page;
        }

        public override void RemovePageFromDisplay(ICLPPage page)
        {
        }

        #endregion //Methods
    }
}

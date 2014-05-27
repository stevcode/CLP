using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class Stamp : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="Stamp" /> from scratch.
        /// </summary>
        public Stamp() { }

        /// <summary>
        /// Initializes <see cref="Stamp" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Stamp" /> belongs to.</param>
        public Stamp(CLPPage parentPage)
            : base(parentPage) { }

        /// <summary>
        /// Initializes <see cref="Stamp" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Stamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #endregion //Properties

        #region Methods

        #region Overrides of APageObjectBase

        /// <summary>
        /// Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.
        /// </summary>
        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        #endregion

        public override IPageObject Duplicate()
        {
            var newStamp = Clone() as Stamp;
            if(newStamp == null)
            {
                return null;
            }
            newStamp.CreationDate = DateTime.Now;
            newStamp.ID = Guid.NewGuid().ToCompactID();
            newStamp.VersionIndex = 0;
            newStamp.LastVersionIndex = null;
            newStamp.ParentPage = ParentPage;

            return newStamp;
        }

        #endregion //Methods
    }
}
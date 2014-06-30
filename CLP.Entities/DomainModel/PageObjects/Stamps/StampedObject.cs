using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class StampedObject : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="StampedObject" /> from scratch.
        /// </summary>
        public StampedObject() { }

        /// <summary>
        /// Initializes <see cref="StampedObject" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampedObject" /> belongs to.</param>
        public StampedObject(CLPPage parentPage)
            : base(parentPage) { }

        /// <summary>
        /// Initializes <see cref="StampedObject" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StampedObject(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Whether or not the <see cref="StampedObject" /> has been stamped onto the page.
        /// </summary>
        public bool IsStamped
        {
            get { return GetValue<bool>(IsStampedProperty); }
            set { SetValue(IsStampedProperty, value); }
        }

        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool), false);

        /// <summary>
        /// Whether or not the <see cref="StampedObject" /> is a stamp from a collection stamp.
        /// </summary>
        public bool IsStampedCollection
        {
            get { return GetValue<bool>(IsStampedCollectionProperty); }
            set { SetValue(IsStampedCollectionProperty, value); }
        }

        public static readonly PropertyData IsStampedCollectionProperty = RegisterProperty("IsStampedCollection", typeof(bool), false);

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.
        /// </summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public override IPageObject Duplicate()
        {
            var newStampedObject = Clone() as StampedObject;
            if(newStampedObject == null)
            {
                return null;
            }
            newStampedObject.CreationDate = DateTime.Now;
            newStampedObject.ID = Guid.NewGuid().ToCompactID();
            newStampedObject.VersionIndex = 0;
            newStampedObject.LastVersionIndex = null;
            newStampedObject.ParentPage = ParentPage;

            return newStampedObject;
        }

        #endregion //Methods
    }
}
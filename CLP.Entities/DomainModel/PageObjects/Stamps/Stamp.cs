using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class Stamp : APageObjectBase, ICountable
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

        public virtual double HandleHeight
        {
            get { return 35; }
        }

        public virtual double PartsHeight
        {
            get { return 70; }
        }

        /// <summary>
        /// Designates the <see cref="Stamp" /> as a CollectionStamp that can accept <see cref="IPageObject" />s.
        /// </summary>
        public bool IsCollectionStamp
        {
            get { return GetValue<bool>(IsCollectionStampProperty); }
            set { SetValue(IsCollectionStampProperty, value); }
        }

        public static readonly PropertyData IsCollectionStampProperty = RegisterProperty("IsCollectionStamp", typeof(bool), false);

        #region ICountable Members

        /// <summary>
        /// Number of parts the <see cref="Stamp" /> represents.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 0);

        /// <summary>
        /// Is an <see cref="ICountable" /> that doesn't accept inner parts.
        /// </summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof(bool), false);

        #endregion

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

        public override void OnResizing()
        {

        }

        public override void OnResized() { OnResizing(); }

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
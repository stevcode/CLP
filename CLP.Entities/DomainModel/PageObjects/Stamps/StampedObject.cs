using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class StampedObject : APageObjectBase, ICountable //, IPageObjectAccepter
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
        public StampedObject(CLPPage parentPage, string parentStampID, string imageHashID, bool isStampedCollection)
            : base(parentPage)
        {
            ParentStampID = parentStampID;
            ImageHashID = imageHashID;
            IsStampedCollection = isStampedCollection;
        }

        /// <summary>
        /// Initializes <see cref="StampedObject" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampedObject" /> belongs to.</param>
        public StampedObject(CLPPage parentPage, string parentStampID, bool isStampedCollection)
            : this(parentPage, parentStampID, string.Empty, isStampedCollection) { }

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
        /// Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.
        /// </summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public virtual double PartsHeight
        {
            get { return 20; }
        }

        /// <summary>
        /// The Unique Identifier for the <see cref="StampedObject" />'s parent <see cref="Stamp" />.
        /// </summary>
        public string ParentStampID
        {
            get { return GetValue<string>(ParentStampIDProperty); }
            set { SetValue(ParentStampIDProperty, value); }
        }

        public static readonly PropertyData ParentStampIDProperty = RegisterProperty("ParentStampID", typeof(string));

        /// <summary>
        /// The unique Hash of the image this <see cref="StampedObject" /> contains.
        /// </summary>
        public string ImageHashID
        {
            get { return GetValue<string>(ImageHashIDProperty); }
            set { SetValue(ImageHashIDProperty, value); }
        }

        public static readonly PropertyData ImageHashIDProperty = RegisterProperty("ImageHashID", typeof(string), string.Empty);

        /// <summary>
        /// Whether or not the <see cref="StampedObject" /> is a stamp from a collection stamp.
        /// </summary>
        public bool IsStampedCollection
        {
            get { return GetValue<bool>(IsStampedCollectionProperty); }
            set { SetValue(IsStampedCollectionProperty, value); }
        }

        public static readonly PropertyData IsStampedCollectionProperty = RegisterProperty("IsStampedCollection", typeof(bool), false);

        /// <summary>
        /// List of <see cref="StrokePathDTO" />s that make up the <see cref="StampedObject" />.
        /// </summary>
        public List<StrokePathDTO> StrokePaths
        {
            get { return GetValue<List<StrokePathDTO>>(StrokePathsProperty); }
            set { SetValue(StrokePathsProperty, value); }
        }

        public static readonly PropertyData StrokePathsProperty = RegisterProperty("StrokePaths", typeof(List<StrokePathDTO>), () => new List<StrokePathDTO>());

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
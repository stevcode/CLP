using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum StampedObjectTypes
    {
        GeneralStampedObject,
        VisibleParts,
        GroupStampedObject,
        EmptyGroupStampedObject
    }

    [Serializable]
    public class StampedObject : APageObjectBase, ICountable, IPageObjectAccepter
    {
        #region Constructors

        /// <summary>Initializes <see cref="StampedObject" /> from scratch.</summary>
        public StampedObject() { }

        /// <summary>Initializes <see cref="StampedObject" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampedObject" /> belongs to.</param>
        public StampedObject(CLPPage parentPage, string parentStampID, string imageHashID, StampedObjectTypes stampedObjectType)
            : base(parentPage)
        {
            ParentStampID = parentStampID;
            ImageHashID = imageHashID;
            StampedObjectType = stampedObjectType;
            CanAcceptPageObjects = stampedObjectType == StampedObjectTypes.GroupStampedObject || stampedObjectType == StampedObjectTypes.EmptyGroupStampedObject;
        }

        /// <summary>Initializes <see cref="StampedObject" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampedObject" /> belongs to.</param>
        public StampedObject(CLPPage parentPage, string parentStampID, StampedObjectTypes stampedObjectType)
            : this(parentPage, parentStampID, string.Empty, stampedObjectType) { }

        /// <summary>Initializes <see cref="StampedObject" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StampedObject(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int ZIndex
        {
            get { return StampedObjectType == StampedObjectTypes.GroupStampedObject || StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject ? 70 : 80; }
        }

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public virtual double PartsHeight
        {
            get { return 20; }
        }

        /// <summary>The Unique Identifier for the <see cref="StampedObject" />'s parent <see cref="Stamp" />.</summary>
        public string ParentStampID
        {
            get { return GetValue<string>(ParentStampIDProperty); }
            set { SetValue(ParentStampIDProperty, value); }
        }

        public static readonly PropertyData ParentStampIDProperty = RegisterProperty("ParentStampID", typeof (string));

        /// <summary>The unique Hash of the image this <see cref="StampedObject" /> contains.</summary>
        public string ImageHashID
        {
            get { return GetValue<string>(ImageHashIDProperty); }
            set { SetValue(ImageHashIDProperty, value); }
        }

        public static readonly PropertyData ImageHashIDProperty = RegisterProperty("ImageHashID", typeof (string), string.Empty);

        /// <summary>Type of <see cref="StampedObject" />.</summary>
        public StampedObjectTypes StampedObjectType
        {
            get { return GetValue<StampedObjectTypes>(StampedObjectTypeProperty); }
            set { SetValue(StampedObjectTypeProperty, value); }
        }

        public static readonly PropertyData StampedObjectTypeProperty = RegisterProperty("StampedObjectType", typeof (StampedObjectTypes), StampedObjectTypes.GeneralStampedObject);

        /// <summary>List of <see cref="StrokeDTO" />s that make up the <see cref="StampedObject" />.</summary>
        public List<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof (List<StrokeDTO>), () => new List<StrokeDTO>());

        /// <summary>
        /// Toggles the visibility of a boundary around the stampedObject.
        /// </summary>
        public bool IsBoundaryVisible
        {
            get { return GetValue<bool>(IsBoundaryVisibleProperty); }
            set { SetValue(IsBoundaryVisibleProperty, value); }
        }

        public static readonly PropertyData IsBoundaryVisibleProperty = RegisterProperty("IsBoundaryVisible", typeof (bool), true);

        /// <summary>
        /// Toggles visibility of Parts.
        /// </summary>
        public bool IsPartsLabelVisible
        {
            get { return GetValue<bool>(IsPartsLabelVisibleProperty); }
            set { SetValue(IsPartsLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsPartsLabelVisibleProperty = RegisterProperty("IsPartsLabelVisible", typeof (bool), false);

        #region ICountable Members

        /// <summary>Number of parts the <see cref="Stamp" /> represents.</summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int), 0);

        /// <summary>Is an <see cref="ICountable" /> that doesn't accept inner parts.</summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof (bool), false);

        /// <summary>Parts is Auto-Generated and non-modifiable (except under special circumstances).</summary>
        public bool IsPartsAutoGenerated
        {
            get { return GetValue<bool>(IsPartsAutoGeneratedProperty); }
            set { SetValue(IsPartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData IsPartsAutoGeneratedProperty = RegisterProperty("IsPartsAutoGenerated", typeof (bool), false);

        #endregion

        #region IPageObjectAccepter Members

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="IPageObject" />s.</summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof (bool), false);

        /// <summary>The currently accepted <see cref="IPageObject" />s.</summary>
        [XmlIgnore]
        public List<IPageObject> AcceptedPageObjects
        {
            get { return GetValue<List<IPageObject>>(AcceptedPageObjectsProperty); }
            set { SetValue(AcceptedPageObjectsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectsProperty = RegisterProperty("AcceptedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        /// <summary>The IDs of the <see cref="IPageObject" />s that have been accepted.</summary>
        public List<string> AcceptedPageObjectIDs
        {
            get { return GetValue<List<string>>(AcceptedPageObjectIDsProperty); }
            set { SetValue(AcceptedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectIDsProperty = RegisterProperty("AcceptedPageObjectIDs", typeof (List<string>), () => new List<string>());

        #endregion //IPageObjectAccepter Members

        #endregion //Properties

        #region Methods

        public override void OnMoving(double oldX, double oldY)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if (CanAcceptPageObjects)
            {
                foreach (var pageObject in AcceptedPageObjects)
                {
                    pageObject.XPosition += deltaX;
                    pageObject.YPosition += deltaY;
                }
            }
        }

        public override void OnMoved(double oldX, double oldY)
        {
            if (ParentPage.History.IsAnimating)
            {
                return;
            }

            try
            {
                foreach (var acceptorPageObject in ParentPage.PageObjects.OfType<IPageObjectAccepter>().Where(pageObject => pageObject.CanAcceptPageObjects && pageObject.ID != ID))
                {
                    var removedPageObjects = new List<IPageObject>();
                    var addedPageObjects = new ObservableCollection<IPageObject>();

                    if (acceptorPageObject.AcceptedPageObjectIDs.Contains(ID) &&
                        !acceptorPageObject.PageObjectIsOver(this, .50))
                    {
                        removedPageObjects.Add(this);
                    }

                    if (!acceptorPageObject.AcceptedPageObjectIDs.Contains(ID) &&
                        acceptorPageObject.PageObjectIsOver(this, .50))
                    {
                        addedPageObjects.Add(this);
                    }

                    acceptorPageObject.AcceptPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("StampedObject.OnMoved() Exception: " + ex.Message);
            }
            base.OnMoved(oldX, oldY);
        }

        public override IPageObject Duplicate()
        {
            var newStampedObject = Clone() as StampedObject;
            if (newStampedObject == null)
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

        public void RefreshParts()
        {
            Parts = 0;
            foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
            {
                Parts += pageObject.Parts;
            }
        }

        #region IPageObjectAccepter Methods

        public void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in removedPageObjects.Where(pageObject => AcceptedPageObjectIDs.Contains(pageObject.ID)))
            {
                AcceptedPageObjects.Remove(pageObject);
                AcceptedPageObjectIDs.Remove(pageObject.ID);
            }

            foreach (var pageObject in addedPageObjects.OfType<ICountable>())
            {
                if (AcceptedPageObjectIDs.Contains(pageObject.ID) ||
                    pageObject is Stamp ||
                    (pageObject is StampedObject &&
                     ((pageObject as StampedObject).StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject ||
                      (pageObject as StampedObject).StampedObjectType == StampedObjectTypes.GroupStampedObject)))
                {
                    continue;
                }
                AcceptedPageObjects.Add(pageObject);
                AcceptedPageObjectIDs.Add(pageObject.ID);
            }

            RefreshParts();
        }

        public void RefreshAcceptedPageObjects()
        {
            AcceptedPageObjects.Clear();
            AcceptedPageObjectIDs.Clear();
            if (!CanAcceptPageObjects)
            {
                return;
            }

            var pageObjectsOverStamp = from pageObject in ParentPage.PageObjects
                                       where PageObjectIsOver(pageObject, .90) //PageObject must be at least 90% contained by Stamp body.
                                       select pageObject;

            AcceptPageObjects(pageObjectsOverStamp, new List<IPageObject>());
        }

        #endregion //IPageObjectAccepter Methods

        #endregion //Methods
    }
}
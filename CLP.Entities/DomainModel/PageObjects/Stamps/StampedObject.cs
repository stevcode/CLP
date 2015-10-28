using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
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
    public class StampedObject : APageObjectAccepter, ICountable
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

        /// <summary>Toggles the visibility of a boundary around the stampedObject.</summary>
        public bool IsBoundaryVisible
        {
            get { return GetValue<bool>(IsBoundaryVisibleProperty); }
            set { SetValue(IsBoundaryVisibleProperty, value); }
        }

        public static readonly PropertyData IsBoundaryVisibleProperty = RegisterProperty("IsBoundaryVisible", typeof (bool), true);

        /// <summary>Toggles visibility of Parts.</summary>
        public bool IsPartsLabelVisible
        {
            get { return GetValue<bool>(IsPartsLabelVisibleProperty); }
            set { SetValue(IsPartsLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsPartsLabelVisibleProperty = RegisterProperty("IsPartsLabelVisible", typeof (bool), false);

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get
            {
                string stampObjectType;
                switch (StampedObjectType)
                {
                    case StampedObjectTypes.GeneralStampedObject:
                        stampObjectType = "Stamped Object";
                        break;
                    case StampedObjectTypes.VisibleParts:
                        stampObjectType = "Stamped Object";
                        break;
                    case StampedObjectTypes.GroupStampedObject:
                        stampObjectType = "Group Stamped Object";
                        break;
                    case StampedObjectTypes.EmptyGroupStampedObject:
                        stampObjectType = "Empty Group Stamped Object";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return string.Format("{0} with {1} Part(s)", stampObjectType, Parts);
            }
        }

        public override string CodedName
        {
            get
            {
                return "STAMP IMAGE";
            }
        }

        public override string CodedID
        {
            get
            {
                return ""; //pictorial, discrete/unitized, drag/menu
            }
        }
             

        public override int ZIndex
        {
            get { return StampedObjectType == StampedObjectTypes.GroupStampedObject || StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject ? 70 : 80; }
        }

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            if (!CanAcceptPageObjects ||
                !AcceptedPageObjects.Any() ||
                !fromHistory)
            {
                base.OnAdded(fromHistory);
                return;
            }

            var pageObjectsToRestore = new List<IPageObject>();

            foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.History.TrashedPageObjects.Contains(p)))
            {
                pageObjectsToRestore.Add(pageObject);
            }

            ParentPage.PageObjects.AddRange(pageObjectsToRestore);
            foreach (var pageObject in pageObjectsToRestore)
            {
                ParentPage.History.TrashedPageObjects.Remove(pageObject);
            }

            base.OnAdded(fromHistory);
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            if (!CanAcceptPageObjects ||
                !AcceptedPageObjects.Any())
            {
                return;
            }

            var pageObjectsToTrash = new List<IPageObject>();

            foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.PageObjects.Contains(p)))
            {
                pageObjectsToTrash.Add(pageObject);
            }

            foreach (var pageObject in pageObjectsToTrash)
            {
                ParentPage.PageObjects.Remove(pageObject);
            }

            ParentPage.History.TrashedPageObjects.AddRange(pageObjectsToTrash);

            base.OnDeleted(fromHistory);
        }

        public override void OnMoving(double oldX, double oldY, bool fromHistory = false)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in AcceptedPageObjects)
            {
                pageObject.XPosition += deltaX;
                pageObject.YPosition += deltaY;
            }
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
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

                    acceptorPageObject.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mark.OnMoved() Exception: " + ex.Message);
            }

            base.OnMoved(oldX, oldY, fromHistory);
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

        #endregion //APageObjectBase Overrides

        #region APageObjectAccepter Overrides

        public override void ChangeAcceptedPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            base.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);

            RefreshParts();
        }

        public override bool IsPageObjectTypeAcceptedByThisPageObject(IPageObject pageObject)
        {
            var stampedObject = pageObject as StampedObject;
            if (stampedObject == null)
            {
                return false;
            }

            return stampedObject.StampedObjectType == StampedObjectTypes.GeneralStampedObject;
        }

        #endregion //APageObjectAccepter Overrides

        #region ICountable Implementation

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

        public void RefreshParts()
        {
            Parts = 0;
            foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
            {
                Parts += pageObject.Parts;
            }
        }

        #endregion //ICountable Implementation
    }
}
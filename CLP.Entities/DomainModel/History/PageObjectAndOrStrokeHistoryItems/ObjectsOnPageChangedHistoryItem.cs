using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectsOnPageChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> from scratch.</summary>
        public ObjectsOnPageChangedHistoryItem() { }

        public ObjectsOnPageChangedHistoryItem(CLPPage parentPage,
                                               Person owner,
                                               IEnumerable<IPageObject> pageObjectsAdded,
                                               IEnumerable<IPageObject> pageObjectsRemoved) 
            : this(parentPage, owner, pageObjectsAdded, pageObjectsRemoved, new List<Stroke>(), new List<Stroke>())
        { }

        public ObjectsOnPageChangedHistoryItem(CLPPage parentPage,
                                               Person owner,
                                               IEnumerable<Stroke> strokesAdded,
                                               IEnumerable<Stroke> strokesRemoved)
            : this(parentPage, owner, new List<IPageObject>(), new List<IPageObject>(), strokesAdded, strokesRemoved)
        { }

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public ObjectsOnPageChangedHistoryItem(CLPPage parentPage,
                                               Person owner,
                                               IEnumerable<IPageObject> pageObjectsAdded,
                                               IEnumerable<IPageObject> pageObjectsRemoved,
                                               IEnumerable<Stroke> strokesAdded,
                                               IEnumerable<Stroke> strokesRemoved)
            : base(parentPage, owner)
        {
            PageObjectIDsAdded = pageObjectsAdded.Select(p => p.ID).ToList();
            foreach (var pageObject in pageObjectsRemoved)
            {
                PageObjectIDsRemoved.Add(pageObject.ID);
                ParentPage.History.TrashedPageObjects.Add(pageObject);
            }

            StrokeIDsAdded = strokesAdded.Select(s => s.GetStrokeID()).ToList();
            foreach (var stroke in strokesRemoved)
            {
                StrokeIDsRemoved.Add(stroke.GetStrokeID());
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected ObjectsOnPageChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Obsolete Constructors

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> from <see cref="StrokesChangedHistoryItem" />.</summary>
        public ObjectsOnPageChangedHistoryItem(StrokesChangedHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            VersionIndex = obsoleteHistoryItem.VersionIndex;
            LastVersionIndex = obsoleteHistoryItem.LastVersionIndex;
            DifferentiationGroup = obsoleteHistoryItem.DifferentiationGroup;
            ParentPage = obsoleteHistoryItem.ParentPage;

            PageObjectIDsAdded = new List<string>();
            StrokeIDsAdded = obsoleteHistoryItem.StrokeIDsAdded;
            StrokeIDsRemoved = obsoleteHistoryItem.StrokeIDsRemoved;
        }

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> from <see cref="PageObjectsAddedHistoryItem" />.</summary>
        public ObjectsOnPageChangedHistoryItem(PageObjectsAddedHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            VersionIndex = obsoleteHistoryItem.VersionIndex;
            LastVersionIndex = obsoleteHistoryItem.LastVersionIndex;
            DifferentiationGroup = obsoleteHistoryItem.DifferentiationGroup;
            ParentPage = obsoleteHistoryItem.ParentPage;

            PageObjectIDsAdded = obsoleteHistoryItem.PageObjectIDs; 
            StrokeIDsAdded = new List<string>();
        }

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryItem" /> from <see cref="PageObjectsRemovedHistoryItem" />.</summary>
        public ObjectsOnPageChangedHistoryItem(PageObjectsRemovedHistoryItem obsoleteHistoryItem)
        {
            ID = obsoleteHistoryItem.ID;
            OwnerID = obsoleteHistoryItem.OwnerID;
            VersionIndex = obsoleteHistoryItem.VersionIndex;
            LastVersionIndex = obsoleteHistoryItem.LastVersionIndex;
            DifferentiationGroup = obsoleteHistoryItem.DifferentiationGroup;
            ParentPage = obsoleteHistoryItem.ParentPage;

            PageObjectIDsRemoved = obsoleteHistoryItem.PageObjectIDs;
            PageObjectIDsAdded = new List<string>();
            StrokeIDsAdded = new List<string>();
        } 

        #endregion //Obsolete Constructors

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                if (IsUsingPageObjects && IsUsingStrokes)
                {
                    return 250;
                }

                if (IsUsingStrokes && !IsUsingPageObjects)
                {
                    return 100;
                }

                return 400;
            }
        }

        #region PageObjects

        /// <summary>Unique IDs of the pageObjects added.</summary>
        public List<string> PageObjectIDsAdded
        {
            get { return GetValue<List<string>>(PageObjectIDsAddedProperty); }
            set { SetValue(PageObjectIDsAddedProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsAddedProperty = RegisterProperty("PageObjectIDsAdded", typeof (List<string>));

        /// <summary>Unique IDs of the pageObjects removed.</summary>
        public List<string> PageObjectIDsRemoved
        {
            get { return GetValue<List<string>>(PageObjectIDsRemovedProperty); }
            set { SetValue(PageObjectIDsRemovedProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsRemovedProperty = RegisterProperty("PageObjectIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>List of the pageObjects that were removed from the page as a result of the UndoAction(). Cleared on Redo().</summary>
        [XmlIgnore]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        #endregion //PageObjects

        #region Strokes

        /// <summary>Unique IDs of the strokes added.</summary>
        public List<string> StrokeIDsAdded
        {
            get { return GetValue<List<string>>(StrokeIDsAddedProperty); }
            set { SetValue(StrokeIDsAddedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsAddedProperty = RegisterProperty("StrokeIDsAdded", typeof (List<string>));

        /// <summary>Unique IDs of the strokes removed.</summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>List of serialized <see cref="Stroke" />s to be used on another machine when <see cref="StrokesChangedHistoryItem" /> is unpacked.</summary>
        [XmlIgnore]
        public List<StrokeDTO> PackagedSerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(PackagedSerializedStrokesProperty); }
            set { SetValue(PackagedSerializedStrokesProperty, value); }
        }

        public static readonly PropertyData PackagedSerializedStrokesProperty = RegisterProperty("PackagedSerializedStrokes", typeof (List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Strokes

        #region Calculated Properties

        public bool IsUsingPageObjects
        {
            get { return PageObjectIDsAdded.Any() || PageObjectIDsRemoved.Any(); }
        }

        public bool IsUsingStrokes
        {
            get { return StrokeIDsAdded.Any() || StrokeIDsRemoved.Any(); }
        }

        #endregion //Calculated Properties

        public override string FormattedValue
        {
            get
            {
                List<string> PageObjectTypesAdded = new List<string>();
                List<string> PageObjectTypesRemoved = new List<string>();
            
                foreach (var pageObject in PageObjectIDsAdded.Select(pageObjectID => ParentPage.GetPageObjectByIDOnPageOrInHistory(pageObjectID)))
                {
                    if(pageObject == null)
                    {
                        continue;
                    }
                    PageObjectTypesAdded.Add(pageObject.GetType().Name);
                }
                var objectsAdded = string.Empty;
                if(PageObjectTypesAdded.Any())
                {
                    objectsAdded = string.Format("Added {0} on page. ", string.Join(", ", PageObjectTypesAdded));
                    if (PageObjectTypesAdded.Count == 1 && PageObjectTypesAdded[0] == "CLPArray")
                    {
                        var addedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectIDsAdded[0]) as CLPArray;
                        objectsAdded = string.Format("Added CLPArray [{0} x {1}] on page. ", addedArray.Rows, addedArray.Columns);
                    }
                }


                foreach (var pageObject in PageObjectIDsRemoved.Select(pageObjectID => ParentPage.GetPageObjectByIDOnPageOrInHistory(pageObjectID)))
                {
                    if (pageObject == null)
                    {
                        continue;
                    }
                    PageObjectTypesRemoved.Add(pageObject.GetType().Name);
                }
                var objectsRemoved = string.Empty;
                if(PageObjectTypesRemoved.Any())
                {
                    objectsRemoved = string.Format("Removed {0} on page. ", string.Join(", ", PageObjectTypesRemoved));
                    if (PageObjectTypesRemoved.Count == 1 && PageObjectTypesRemoved[0] == "CLPArray")
                    {
                        var removedArray = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectIDsRemoved[0]) as CLPArray;
                        objectsRemoved = string.Format("Removed CLPArray [{0} x {1}] on page. ", removedArray.Rows, removedArray.Columns);
                    }
                }

                var strokesAdded = string.Empty;
                if(StrokeIDsAdded.Any())
                {
                    if(StrokeIDsAdded.Count == 1){
                        strokesAdded = "Added 1 stroke.";
                    }else{
                        strokesAdded = string.Format("Added {0} strokes. ", StrokeIDsAdded.Count);
                    }
                    
                }    

                var strokesRemoved = string.Empty;
                if(StrokeIDsRemoved.Any())
                {
                    if (StrokeIDsRemoved.Count == 1)
                    {
                        strokesRemoved = "Removed 1 stroke.";
                    }else {
                        strokesRemoved = string.Format("Removed {0} strokes. ", StrokeIDsRemoved.Count);
                    }         
                }

                var formattedValue = string.Format("Index # {0}, {1}{2}{3}{4}",
                    HistoryIndex, objectsAdded, objectsRemoved, strokesAdded, strokesRemoved);
                return formattedValue;
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            UndoAction(false);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if (!IsUsingPageObjects &&
                !IsUsingStrokes)
            {
                Console.WriteLine("ERROR: No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                return;
            }

            foreach (var pageObject in PageObjectIDsAdded.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("ERROR: Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.PageObjects.Remove(pageObject);
                pageObject.OnDeleted(true);
                ParentPage.History.TrashedPageObjects.Add(pageObject);
            }

            foreach (var pageObject in PageObjectIDsRemoved.Select(id => ParentPage.GetVerifiedPageObjectInTrashByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("ERROR: Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.History.TrashedPageObjects.Remove(pageObject);
                ParentPage.PageObjects.Add(pageObject);
                pageObject.OnAdded(true);
            }

            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in ParentPage.PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.ChangeAcceptedStrokes(addedStrokes, removedStrokes);
            }
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if (!IsUsingPageObjects &&
                !IsUsingStrokes)
            {
                Console.WriteLine("ERROR: No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                return;
            }

            foreach (var pageObject in PageObjectIDsRemoved.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("ERROR: Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.PageObjects.Remove(pageObject);
                pageObject.OnDeleted(true);
                ParentPage.History.TrashedPageObjects.Add(pageObject);
            }

            foreach (var pageObject in PageObjectIDsAdded.Select(id => ParentPage.GetVerifiedPageObjectInTrashByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("ERROR: Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.History.TrashedPageObjects.Remove(pageObject);
                ParentPage.PageObjects.Add(pageObject);
                pageObject.OnAdded(true);
            }

            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in ParentPage.PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.ChangeAcceptedStrokes(addedStrokes, removedStrokes);
            }
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            PackagedSerializedStrokes.Clear();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                PackagedSerializedStrokes.Add(stroke.ToStrokeDTO());
            }
            return this;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem()
        {
            foreach (var stroke in PackagedSerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke()))
            {
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        public override bool IsUsingTrashedPageObject(string id) { return PageObjectIDsRemoved.Contains(id) || PageObjectIDsAdded.Contains(id); }

        public override bool IsUsingTrashedInkStroke(string id) { return StrokeIDsRemoved.Contains(id) || StrokeIDsAdded.Contains(id); }

        #endregion //Methods
    }
}
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

        public ObjectsOnPageChangedHistoryItem(CLPPage parentPage, Person owner, IEnumerable<IPageObject> pageObjectsAdded, IEnumerable<IPageObject> pageObjectsRemoved)
            : this(parentPage, owner, pageObjectsAdded, pageObjectsRemoved, new List<Stroke>(), new List<Stroke>()) { }

        public ObjectsOnPageChangedHistoryItem(CLPPage parentPage, Person owner, IEnumerable<Stroke> strokesAdded, IEnumerable<Stroke> strokesRemoved)
            : this(parentPage, owner, new List<IPageObject>(), new List<IPageObject>(), strokesAdded, strokesRemoved) { }

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

        public static readonly PropertyData PageObjectIDsAddedProperty = RegisterProperty("PageObjectIDsAdded", typeof (List<string>), () => new List<string>());

        /// <summary>Unique IDs of the pageObjects removed.</summary>
        public List<string> PageObjectIDsRemoved
        {
            get { return GetValue<List<string>>(PageObjectIDsRemovedProperty); }
            set { SetValue(PageObjectIDsRemovedProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsRemovedProperty = RegisterProperty("PageObjectIDsRemoved", typeof (List<string>), () => new List<string>());

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

        public static readonly PropertyData StrokeIDsAddedProperty = RegisterProperty("StrokeIDsAdded", typeof (List<string>), () => new List<string>());

        /// <summary>Unique IDs of the strokes removed.</summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof (List<string>), () => new List<string>());

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
                var pageObjectsAdded = PageObjectIDsAdded.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList();
                var pageObjectsRemoved = PageObjectIDsRemoved.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList();

                var objectsAdded = pageObjectsAdded.Any() ? string.Format(" Added {0}.", string.Join(",", pageObjectsAdded.Select(p => p.FormattedName))) : string.Empty;
                var objectsRemoved = pageObjectsRemoved.Any() ? string.Format(" Removed {0}.", string.Join(",", pageObjectsRemoved.Select(p => p.FormattedName))) : string.Empty;

                var strokesAdded = StrokeIDsAdded.Any() ? StrokeIDsAdded.Count == 1 ? " Added 1 stroke." : string.Format(" Added {0} strokes.", StrokeIDsAdded.Count) : string.Empty;
                var strokesRemoved = StrokeIDsRemoved.Any()
                                         ? StrokeIDsRemoved.Count == 1 ? " Removed 1 stroke." : string.Format(" Removed {0} strokes.", StrokeIDsRemoved.Count)
                                         : string.Empty;

                return string.Format("Index #{0},{1}{2}{3}{4}", HistoryIndex, objectsAdded, objectsRemoved, strokesAdded, strokesRemoved);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction() { UndoAction(false); }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if (!IsUsingPageObjects &&
                !IsUsingStrokes)
            {
                Console.WriteLine("[ERROR] on Index #{0}, No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                return;
            }

            foreach (var pageObject in PageObjectIDsAdded.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                ParentPage.History.TrashedPageObjects.Remove(pageObject);
                ParentPage.PageObjects.Add(pageObject);
                pageObject.OnAdded(true);
            }

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }

            var removedStrokes = new List<Stroke>();
            foreach (var stroke in StrokeIDsRemoved.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                removedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in ParentPage.PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.ChangeAcceptedStrokes(new List<Stroke>(), addedStrokes);
            }

            foreach (var stroke in removedStrokes)
            {
                var validStrokeAccepters =
                    ParentPage.PageObjects.OfType<IStrokeAccepter>()
                              .Where(p => (p.CreatorID == ParentPage.OwnerID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke))
                              .ToList();

                IStrokeAccepter closestPageObject = null;
                foreach (var pageObject in validStrokeAccepters)
                {
                    if (closestPageObject == null)
                    {
                        closestPageObject = pageObject;
                        continue;
                    }

                    if (closestPageObject.PercentageOfStrokeOverPageObject(stroke) < pageObject.PercentageOfStrokeOverPageObject(stroke))
                    {
                        closestPageObject = pageObject;
                    }
                }

                if (closestPageObject == null)
                {
                    continue;
                }

                closestPageObject.ChangeAcceptedStrokes(new List<Stroke>
                                                        {
                                                            stroke
                                                        },
                                                        new List<Stroke>());
            }
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if (!IsUsingPageObjects &&
                !IsUsingStrokes)
            {
                Console.WriteLine("[ERROR] on Index #{0}, No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                return;
            }

            foreach (var pageObject in PageObjectIDsRemoved.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryItem.", HistoryIndex);
                    continue;
                }
                addedStrokes.Add(stroke);
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in ParentPage.PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.ChangeAcceptedStrokes(new List<Stroke>(), removedStrokes);
            }

            foreach (var stroke in addedStrokes)
            {
                var validStrokeAccepters =
                    ParentPage.PageObjects.OfType<IStrokeAccepter>()
                              .Where(p => (p.CreatorID == ParentPage.OwnerID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke))
                              .ToList();

                IStrokeAccepter closestPageObject = null;
                foreach (var pageObject in validStrokeAccepters)
                {
                    if (closestPageObject == null)
                    {
                        closestPageObject = pageObject;
                        continue;
                    }

                    if (closestPageObject.PercentageOfStrokeOverPageObject(stroke) < pageObject.PercentageOfStrokeOverPageObject(stroke))
                    {
                        closestPageObject = pageObject;
                    }
                }

                if (closestPageObject == null)
                {
                    continue;
                }

                closestPageObject.ChangeAcceptedStrokes(new List<Stroke>
                                                        {
                                                            stroke
                                                        },
                                                        new List<Stroke>());
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectsOnPageChangedHistoryAction : AHistoryActionBase, IStrokesOnPageChangedHistoryAction
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryAction" /> from scratch.</summary>
        public ObjectsOnPageChangedHistoryAction() { }

        public ObjectsOnPageChangedHistoryAction(CLPPage parentPage, Person owner, IEnumerable<IPageObject> pageObjectsAdded, IEnumerable<IPageObject> pageObjectsRemoved)
            : this(parentPage, owner, pageObjectsAdded, pageObjectsRemoved, new List<Stroke>(), new List<Stroke>()) { }

        public ObjectsOnPageChangedHistoryAction(CLPPage parentPage, Person owner, IEnumerable<Stroke> strokesAdded, IEnumerable<Stroke> strokesRemoved)
            : this(parentPage, owner, new List<IPageObject>(), new List<IPageObject>(), strokesAdded, strokesRemoved) { }

        /// <summary>Initializes <see cref="ObjectsOnPageChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public ObjectsOnPageChangedHistoryAction(CLPPage parentPage,
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

        #endregion // Constructors

        #region Properties

        #region PageObjects

        /// <summary>Unique IDs of the pageObjects added.</summary>
        public List<string> PageObjectIDsAdded
        {
            get { return GetValue<List<string>>(PageObjectIDsAddedProperty); }
            set { SetValue(PageObjectIDsAddedProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsAddedProperty = RegisterProperty("PageObjectIDsAdded", typeof(List<string>), () => new List<string>());

        /// <summary>Unique IDs of the pageObjects removed.</summary>
        public List<string> PageObjectIDsRemoved
        {
            get { return GetValue<List<string>>(PageObjectIDsRemovedProperty); }
            set { SetValue(PageObjectIDsRemovedProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsRemovedProperty = RegisterProperty("PageObjectIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>List of the pageObjects that were removed from the page as a result of the UndoAction(). Cleared on Redo().</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<IPageObject> PackagedPageObjects
        {
            get { return GetValue<List<IPageObject>>(PackagedPageObjectsProperty); }
            set { SetValue(PackagedPageObjectsProperty, value); }
        }

        public static readonly PropertyData PackagedPageObjectsProperty = RegisterProperty("PackagedPageObjects", typeof(List<IPageObject>), () => new List<IPageObject>());

        #endregion // PageObjects

        #region Strokes

        /// <summary>Unique IDs of the strokes added.</summary>
        public List<string> StrokeIDsAdded
        {
            get { return GetValue<List<string>>(StrokeIDsAddedProperty); }
            set { SetValue(StrokeIDsAddedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsAddedProperty = RegisterProperty("StrokeIDsAdded", typeof(List<string>), () => new List<string>());

        /// <summary>Unique IDs of the strokes removed.</summary>
        public List<string> StrokeIDsRemoved
        {
            get { return GetValue<List<string>>(StrokeIDsRemovedProperty); }
            set { SetValue(StrokeIDsRemovedProperty, value); }
        }

        public static readonly PropertyData StrokeIDsRemovedProperty = RegisterProperty("StrokeIDsRemoved", typeof(List<string>), () => new List<string>());

        /// <summary>List of serialized <see cref="Stroke" />s to be used on another machine when <see cref="ObjectsOnPageChangedHistoryAction" /> is unpacked.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<StrokeDTO> PackagedSerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(PackagedSerializedStrokesProperty); }
            set { SetValue(PackagedSerializedStrokesProperty, value); }
        }

        public static readonly PropertyData PackagedSerializedStrokesProperty = RegisterProperty("PackagedSerializedStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion // Strokes

        #region Calculated Properties

        public bool IsUsingPageObjects => PageObjectIDsAdded.Any() || PageObjectIDsRemoved.Any();

        public bool IsUsingStrokes => StrokeIDsAdded.Any() || StrokeIDsRemoved.Any();

        public List<IPageObject> PageObjectsAdded
        {
            get { return PageObjectIDsAdded.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList(); }
        }

        public List<IPageObject> PageObjectsRemoved
        {
            get { return PageObjectIDsRemoved.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList(); }
        }

        public List<Stroke> StrokesAdded
        {
            get { return StrokeIDsAdded.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList(); }
        }

        public List<Stroke> StrokesRemoved
        {
            get { return StrokeIDsRemoved.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList(); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region AHistoryActionBase Overrides

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

        protected override string FormattedReport
        {
            get
            {
                var pageObjectsAdded = PageObjectsAdded;
                var pageObjectsRemoved = PageObjectsRemoved;

                var objectsAdded = pageObjectsAdded.Any() ? $" Added {string.Join(",", pageObjectsAdded.Select(p => p.FormattedName))}." : string.Empty;
                var objectsRemoved = pageObjectsRemoved.Any() ? $" Removed {string.Join(",", pageObjectsRemoved.Select(p => p.FormattedName))}." : string.Empty;

                var strokesAdded = StrokeIDsAdded.Any() ? StrokeIDsAdded.Count == 1 ? " Added 1 stroke." : $" Added {StrokeIDsAdded.Count} strokes." : string.Empty;
                var strokesRemoved = StrokeIDsRemoved.Any() ? StrokeIDsRemoved.Count == 1 ? " Removed 1 stroke." : $" Removed {StrokeIDsRemoved.Count} strokes." : string.Empty;

                return $"{objectsAdded}{objectsRemoved}{strokesAdded}{strokesRemoved}";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            if (!IsUsingPageObjects &&
                !IsUsingStrokes)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryAction.");
                return;
            }

            foreach (var pageObject in PageObjectIDsAdded.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryAction.");
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
                    ParentPage.PageObjects.OfType<IStrokeAccepter>().Where(p => (p.CreatorID == ParentPage.OwnerID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke)).ToList();

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

                closestPageObject?.ChangeAcceptedStrokes(new List<Stroke>
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
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, No strokes or pageObjects Added or Removed in ObjectsOnPageChangedHistoryAction.");
                return;
            }

            foreach (var pageObject in PageObjectIDsRemoved.Select(id => ParentPage.GetVerifiedPageObjectOnPageByID(id)))
            {
                if (pageObject == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null pageObject in PageObjectIDsAdded in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null pageObject in PageObjectIDsRemoved in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null stroke in StrokeIDsRemoved in ObjectsOnPageChangedHistoryAction.");
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
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Null stroke in StrokeIDsAdded in ObjectsOnPageChangedHistoryAction.");
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
                    ParentPage.PageObjects.OfType<IStrokeAccepter>().Where(p => (p.CreatorID == ParentPage.OwnerID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke)).ToList();

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

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            PackagedSerializedStrokes.Clear();
            foreach (var stroke in StrokeIDsAdded.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                PackagedSerializedStrokes.Add(stroke.ToStrokeDTO());
            }
            return this;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction()
        {
            foreach (var stroke in PackagedSerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke()))
            {
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return PageObjectIDsRemoved.Contains(id) || PageObjectIDsAdded.Contains(id);
        }

        public override bool IsUsingTrashedInkStroke(string id)
        {
            return StrokeIDsRemoved.Contains(id) || StrokeIDsAdded.Contains(id);
        }

        #endregion // AHistoryActionBase Overrides
    }
}
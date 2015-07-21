using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public abstract class AStrokeAccepter : APageObjectBase, IStrokeAccepter
    {
        #region Constructors

        protected AStrokeAccepter() { }

        protected AStrokeAccepter(CLPPage parentPage)
            : base(parentPage) { }

        public AStrokeAccepter(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region IStrokeAccepter Implementation

        /// <summary>Stroke must be at least this percent contained by StrokeAcceptanceBoundingBox.</summary>
        public virtual int StrokeHitTestPercentage
        {
            get { return 95; }
        }

        public virtual Rect StrokeAcceptanceBoundingBox
        {
            get { return new Rect(XPosition, YPosition, Width, Height); }
        }

        /// <summary>Determines whether the <see cref="IStrokeAccepter" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof (bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes", typeof (List<Stroke>), () => new List<Stroke>());

        /// <summary>The IDs of the <see cref="Stroke" />s that have been accepted.</summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs", typeof (List<string>), () => new List<string>());

        public void LoadAcceptedStrokes()
        {
            if (!AcceptedStrokeParentIDs.Any())
            {
                return;
            }

            AcceptedStrokes = AcceptedStrokeParentIDs.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList();
        }

        public virtual void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            // Remove Strokes
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            foreach (var stroke in removedStrokesList.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
            }

            // Add Strokes
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            foreach (var stroke in addedStrokesList.Where(stroke => !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }
        }

        public bool IsStrokeOverPageObject(Stroke stroke) { return stroke.HitTest(StrokeAcceptanceBoundingBox, StrokeHitTestPercentage); }

        public double PercentageOfStrokeOverPageObject(Stroke stroke) { return stroke.PercentContainedByBounds(StrokeAcceptanceBoundingBox); }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where IsStrokeOverPageObject(stroke)
                                    select stroke;

            return new StrokeCollection(strokesOverObject);
        }

        #endregion //IStrokeAccepter Implementation

        public static void SplitAcceptedStrokes(List<IPageObject> oldPageObjects, List<IPageObject> newPageObjects, bool isStrokeSingleCapture = true)
        {
            foreach (var oldPageObject in oldPageObjects)
            {
                var strokeAccepter = oldPageObject as IStrokeAccepter;
                if (strokeAccepter == null)
                {
                    return;
                }

                foreach (var stroke in strokeAccepter.AcceptedStrokes)
                {
                    var validStrokeAccepters =
                        newPageObjects.OfType<IStrokeAccepter>()
                                      .Where(p => (p.CreatorID == strokeAccepter.CreatorID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke))
                                      .ToList();
                    if (isStrokeSingleCapture)
                    {
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
            }
        }
    }
}
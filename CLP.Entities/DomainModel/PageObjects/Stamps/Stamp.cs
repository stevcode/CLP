using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class Stamp : APageObjectBase, ICountable, IStrokeAccepter
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
        public Stamp(CLPPage parentPage, string imageHashID, bool isCollectionStamp)
            : base(parentPage)
        {
            IsCollectionStamp = isCollectionStamp;
            Width = isCollectionStamp ? 125 : 75;
            Height = isCollectionStamp ? 230 : 180;
        }

        /// <summary>
        /// Initializes <see cref="Stamp" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Stamp" /> belongs to.</param>
        public Stamp(CLPPage parentPage, bool isCollectionStamp)
            : this(parentPage, string.Empty, isCollectionStamp) { }

        /// <summary>
        /// Initializes <see cref="Stamp" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Stamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.
        /// </summary>
        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

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

        #region IStrokeAccepter Members

        /// <summary>
        /// Determines whether the <see cref="Stamp" /> can currently accept <see cref="Stroke" />s.
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), true);

        /// <summary>
        /// The currently accepted <see cref="Stroke" />s.
        /// </summary>
        [XmlIgnore]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes", typeof(List<Stroke>), () => new List<Stroke>());

        /// <summary>
        /// The IDs of the <see cref="Stroke" />s that have been accepted.
        /// </summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs", typeof(List<string>), () => new List<string>());

        #endregion //IStrokeAccepter Members

        #endregion //Properties

        #region Methods

        public override void OnResizing(double oldWidth, double oldHeight)
        {
        }

        public override void OnResized(double oldWidth, double oldHeight) { OnResizing(oldWidth, oldHeight); }

        public override void OnMoving(double oldX, double oldY)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if(!CanAcceptStrokes)
            {
                return;
            }

            Parallel.ForEach(AcceptedStrokes,
                             stroke =>
                             {
                                 var transform = new Matrix();
                                 transform.Translate(deltaX, deltaY);
                                 stroke.Transform(transform, true);
                             });
        }

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

        #region IStrokeAccepter Methods

        public void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if(!CanAcceptStrokes)
            {
                return;
            }

            foreach(var stroke in removedStrokes.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
            }

            var stampBodyBoundingBox = new Rect(XPosition, YPosition + HandleHeight, Width, Height - HandleHeight - PartsHeight);
            foreach(var stroke in addedStrokes.Where(stroke => stroke.HitTest(stampBodyBoundingBox, 50) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }
        }

        public void RefreshAcceptedStrokes()
        {
            AcceptedStrokes.Clear();
            AcceptedStrokeParentIDs.Clear();
            if(!CanAcceptStrokes)
            {
                return;
            }

            var stampBodyBoundingBox = new Rect(XPosition, YPosition + HandleHeight, Width, Height - HandleHeight - PartsHeight);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(stampBodyBoundingBox, 50) //Stroke must be at least 50 contained by Stamp body.
                                    select stroke;

            AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
        }

        #endregion //IStrokeAccepter Methods

        #endregion //Methods
    }
}
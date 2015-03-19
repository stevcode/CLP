using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class LassoRegion : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> from scratch.
        /// </summary>
        public LassoRegion() { }

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="LassoRegion" /> belongs to.</param>
        public LassoRegion(CLPPage parentPage, List<IPageObject> pageObjects, StrokeCollection inkStrokes, double xPosition, double yPosition, double height, double width)
            : base(parentPage)
        {
            LassoedPageObjects = pageObjects;
            LassoedStrokes = inkStrokes;
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
        }

        /// <summary>
        /// Initializes <see cref="LassoRegion" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public LassoRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int ZIndex { get { return 1000; } }

        /// <summary>
        /// List of all the lassoed <see cref="IPageObject" />s.
        /// </summary>
        public List<IPageObject> LassoedPageObjects
        {
            get { return GetValue<List<IPageObject>>(LassoedPageObjectsProperty); }
            set { SetValue(LassoedPageObjectsProperty, value); }
        }

        public static readonly PropertyData LassoedPageObjectsProperty = RegisterProperty("LassoedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        /// <summary>
        /// StrokeCollection of all the lassoed <see cref="Stroke" />s.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public StrokeCollection LassoedStrokes
        {
            get { return GetValue<StrokeCollection>(LassoedStrokesProperty); }
            set { SetValue(LassoedStrokesProperty, value); }
        }

        public static readonly PropertyData LassoedStrokesProperty = RegisterProperty("LassoedStrokes", typeof (StrokeCollection), () => new StrokeCollection());

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newLassoRegion = Clone() as LassoRegion;
            if(newLassoRegion == null)
            {
                return null;
            }
            newLassoRegion.CreationDate = DateTime.Now;
            newLassoRegion.ID = Guid.NewGuid().ToString();
            newLassoRegion.VersionIndex = 0;
            newLassoRegion.LastVersionIndex = null;
            newLassoRegion.ParentPage = ParentPage;

            return newLassoRegion;
        }

        public override void OnMoving(double oldX, double oldY, bool fromHistory = false)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            foreach (var stroke in LassoedStrokes)
            {
                var transform = new Matrix();
                transform.Translate(deltaX, deltaY);
                stroke.Transform(transform, true);
            }

            foreach (var pageObject in LassoedPageObjects)
            {
                pageObject.XPosition += deltaX;
                pageObject.YPosition += deltaY;
            }
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            if (ParentPage.History.IsAnimating)
            {
                return;
            }

            try
            {
                foreach (var acceptorPageObject in ParentPage.PageObjects.OfType<IPageObjectAccepter>().Where(pageObject => pageObject.CanAcceptPageObjects && pageObject.ID != ID && !LassoedPageObjects.Contains(pageObject)))
                {
                    var removedPageObjects = new List<IPageObject>();
                    var addedPageObjects = new ObservableCollection<IPageObject>();

                    foreach (var lassoedPageObject in LassoedPageObjects)
                    {
                        if (acceptorPageObject.AcceptedPageObjectIDs.Contains(lassoedPageObject.ID) && !acceptorPageObject.PageObjectIsOver(lassoedPageObject, .50))
                        {
                            removedPageObjects.Add(lassoedPageObject);
                        }

                        if (!acceptorPageObject.AcceptedPageObjectIDs.Contains(lassoedPageObject.ID) && acceptorPageObject.PageObjectIsOver(lassoedPageObject, .50))
                        {
                            addedPageObjects.Add(lassoedPageObject);
                        }
                    }

                    acceptorPageObject.AcceptPageObjects(addedPageObjects, removedPageObjects);
                }

                foreach (var acceptorPageObject in ParentPage.PageObjects.OfType<IStrokeAccepter>().Where(pageObject => pageObject.CanAcceptStrokes && pageObject.ID != ID && !LassoedPageObjects.Contains(pageObject)))
                {
                    var pageObjectBounds = new Rect(acceptorPageObject.XPosition, acceptorPageObject.YPosition, acceptorPageObject.Width, acceptorPageObject.Height);

                    var addedStrokesOverObject = from stroke in LassoedStrokes
                                                 where stroke.HitTest(pageObjectBounds, 3)
                                                 select stroke;

                    var pageObject = acceptorPageObject;
                    var removedStrokesOverObject = from stroke in LassoedStrokes
                                                   where pageObject.AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID()) && !stroke.HitTest(pageObjectBounds, 3) 
                                                   select stroke;

                    var addStrokes = new StrokeCollection(addedStrokesOverObject);
                    var removeStrokes = new StrokeCollection(removedStrokesOverObject);
                    acceptorPageObject.AcceptStrokes(addStrokes, removeStrokes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LassoRegion.OnMoved() Exception: " + ex.Message);
            }
        }

        #endregion //Methods
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public abstract class AStrokeAndPageObjectAccepter : AStrokeAccepter, IPageObjectAccepter
    {
        #region Constructors

        protected AStrokeAndPageObjectAccepter() { }

        protected AStrokeAndPageObjectAccepter(CLPPage parentPage)
            : base(parentPage) { }

        public AStrokeAndPageObjectAccepter(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region IPageObjectAccepter Implementation

        /// <summary>PageObject must be at least this percent contained by PageObjectAcceptanceBoundingBox.</summary>
        public virtual int PageObjectHitTestPercentage
        {
            get { return 90; }
        }

        /// <summary>Bounding rectangle used to calculate pageObjects's hit test.</summary>
        public virtual Rect PageObjectAcceptanceBoundingBox
        {
            get { return new Rect(XPosition, YPosition, Width, Height); }
        }

        /// <summary>Determines whether the <see cref="IPageObjectAccepter" /> can currently accept <see cref="IPageObject" />s.</summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof (bool), true);

        /// <summary>The currently accepted <see cref="IPageObject" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
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

        public void LoadAcceptedPageObjects()
        {
            if (!AcceptedPageObjectIDs.Any())
            {
                return;
            }

            AcceptedPageObjects = AcceptedPageObjectIDs.Select(id => ParentPage.GetPageObjectByIDOnPageOrInHistory(id)).Where(p => p != null).ToList();
        }

        public virtual void ChangeAcceptedPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            if (!CanAcceptPageObjects)
            {
                return;
            }

            // Remove PageObjects
            foreach (var pageObject in removedPageObjects.Where(p => AcceptedPageObjectIDs.Contains(p.ID)))
            {
                AcceptedPageObjects.Remove(pageObject);
                AcceptedPageObjectIDs.Remove(pageObject.ID);
            }

            // Add PageObjects
            foreach (var pageObject in addedPageObjects.Where(p => !AcceptedPageObjectIDs.Contains(p.ID) && IsPageObjectTypeAcceptedByThisPageObject(p)))
            {
                AcceptedPageObjects.Add(pageObject);
                AcceptedPageObjectIDs.Add(pageObject.ID);
            }
        }

        public virtual bool IsPageObjectTypeAcceptedByThisPageObject(IPageObject pageObject) { return true; }

        public bool IsPageObjectOverThisPageObject(IPageObject pageObject)
        {
            if (!IsPageObjectTypeAcceptedByThisPageObject(pageObject))
            {
                return false;
            }

            var intersectionArea = PercentageOfPageObjectOverThisPageObject(pageObject);
            if (intersectionArea <= 0.0)
            {
                return false;
            }

            var areaObject = pageObject.Height * pageObject.Width;
            var area = PageObjectAcceptanceBoundingBox.Height * PageObjectAcceptanceBoundingBox.Width;

            return intersectionArea / areaObject >= PageObjectHitTestPercentage / 100.0 || intersectionArea / area >= PageObjectHitTestPercentage / 100.0;
        }

        public double PercentageOfPageObjectOverThisPageObject(IPageObject pageObject)
        {
            if (!IsPageObjectTypeAcceptedByThisPageObject(pageObject))
            {
                return 0.0;
            }

            var top = Math.Max(PageObjectAcceptanceBoundingBox.Y, pageObject.YPosition);
            var bottom = Math.Min(PageObjectAcceptanceBoundingBox.Y + PageObjectAcceptanceBoundingBox.Height, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(PageObjectAcceptanceBoundingBox.X, pageObject.XPosition);
            var right = Math.Min(PageObjectAcceptanceBoundingBox.X + PageObjectAcceptanceBoundingBox.Width, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            if (deltaY <= 0 ||
                deltaX <= 0)
            {
                return 0.0;
            }

            var intersectionArea = deltaY * deltaX;
            return intersectionArea;
        }

        public List<IPageObject> GetPageObjectsOverThisPageObject()
        {
            var pageObjectsOverThisPageObject = from pageObject in ParentPage.PageObjects
                                                where IsPageObjectOverThisPageObject(pageObject)
                                                select pageObject;

            return pageObjectsOverThisPageObject.ToList();
        }

        #endregion //IPageObjectAccepter Implementation
    }
}
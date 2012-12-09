﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;
using System.Windows.Ink;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CLP.Models
{
    [Serializable]
    public class CLPStamp : DataObjectBase<CLPStamp>, ICLPPageObject
    {
        public static double HANDLE_HEIGHT
        {
            get
            {
                return 35;
            }
        }    

        public static double PARTS_SIDE
        {
            get
            {
                return 75;
            }
        }
        

        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject, CLPPage page)
            : base()
        {
            ParentPage = page;
            ParentPageID = page.UniqueID;
            StrokePathContainer = new CLPStrokePathContainer(internalPageObject, page);

            Height = StrokePathContainer.Height + HANDLE_HEIGHT + PARTS_SIDE;
            Width = StrokePathContainer.Width;

            HandwritingRegionParts = new CLPHandwritingRegion(CLPHandwritingAnalysisType.NUMBER, page);
            HandwritingRegionParts.Height = PARTS_SIDE;
            HandwritingRegionParts.Width = PARTS_SIDE;
            HandwritingRegionParts.XPosition = XPosition;
            HandwritingRegionParts.YPosition = YPosition + Height - PARTS_SIDE;

            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            CanAcceptStrokes = true;
            CanAcceptPageObjects = true;
            CLPPageObjectBase.ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPStamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPStrokePathContainer StrokePathContainer
        {
            get { return GetValue<CLPStrokePathContainer>(StrokePathContainerProperty); }
            set { SetValue(StrokePathContainerProperty, value); }
        }

        public static readonly PropertyData StrokePathContainerProperty = RegisterProperty("StrokePathContainer", typeof(CLPStrokePathContainer), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPHandwritingRegion HandwritingRegionParts
        {
            get { return GetValue<CLPHandwritingRegion>(HandwritingRegionPartsProperty); }
            set { SetValue(HandwritingRegionPartsProperty, value); }
        }

        public static readonly PropertyData HandwritingRegionPartsProperty = RegisterProperty("HandwritingRegionParts", typeof(CLPHandwritingRegion), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool PartsAutoGenerated
        {
            get { return GetValue<bool>(PartsAutoGeneratedProperty); }
            set { SetValue(PartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData PartsAutoGeneratedProperty = RegisterProperty("PartsAutoGenerated", typeof(bool), false);

        #endregion //Properties

        #region Methods

        public string PageObjectType
        {
            get { return "CLPStamp"; }
        }

        public ICLPPageObject Duplicate()
        {
            CLPStamp newStamp = this.Clone() as CLPStamp;
            newStamp.UniqueID = Guid.NewGuid().ToString();
            newStamp.ParentPage = ParentPage;
            if(newStamp.StrokePathContainer != null)
            {
            	newStamp.StrokePathContainer.ParentPage = ParentPage;
            }
            if(newStamp.HandwritingRegionParts != null)
            {
                newStamp.HandwritingRegionParts.ParentPage = ParentPage;
            }
            return newStamp;
        }

        public void OnRemoved()
        {
            if(!StrokePathContainer.IsStamped)
            {
                foreach(Stroke stroke in GetStrokesOverPageObject())
                {
                    ParentPage.InkStrokes.Remove(stroke);
                }
            }

            foreach (ICLPPageObject po in GetPageObjectsOverPageObject())
            {
                po.OnRemoved();
                ParentPage.PageObjects.Remove(po);
            }
        }

        public virtual void RefreshStrokeParentIDs()
         {
            if(CanAcceptStrokes)
            {
                PageObjectStrokeParentIDs.Clear();
                HandwritingRegionParts.PageObjectStrokeParentIDs.Clear();

                Rect rect = new Rect(XPosition, YPosition, Width, Height);
                List<string> addedStrokeIDsOverObject = new List<string>();
                foreach(Stroke stroke in ParentPage.InkStrokes)
                {
                    if (stroke.HitTest(rect, 3))
                    {
                        addedStrokeIDsOverObject.Add(stroke.GetPropertyData(CLPPage.StrokeIDKey) as string);
                    }
                }

                AcceptStrokes(addedStrokeIDsOverObject, new List<string>());
            }
        }

        public void AcceptStrokes(List<string> addedStrokes, List<string> removedStrokes)
        {
            foreach(string strokeID in removedStrokes)
            {
                try
                {
                    Console.WriteLine("RemoveStroke " + strokeID);
                    PageObjectStrokeParentIDs.Remove(strokeID);
                    HandwritingRegionParts.PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }

            var strokes =
                from strokeID in addedStrokes
                from stroke in ParentPage.InkStrokes
                where (stroke.GetPropertyData(CLPPage.StrokeIDKey) as string).Equals(strokeID)
                select stroke;
            Rect rectParts = new Rect(HandwritingRegionParts.XPosition, HandwritingRegionParts.YPosition,
                HandwritingRegionParts.Width, HandwritingRegionParts.Height);
            List<string> handwritingRegionStrokeIDsAdd = new List<string>();
            foreach(Stroke stroke in new StrokeCollection(strokes.ToList()))
            {
                if(stroke.HitTest(rectParts, 3))
                {
                    Console.WriteLine("Accept Strokes add hw");
                    handwritingRegionStrokeIDsAdd.Add((stroke.GetPropertyData(CLPPage.StrokeIDKey) as string));
                    PartsAutoGenerated = false;
                }
                else
                {
                    ResetParts();
                }
                PageObjectStrokeParentIDs.Add(stroke.GetPropertyData(CLPPage.StrokeIDKey) as string);
            }
            HandwritingRegionParts.AcceptStrokes(handwritingRegionStrokeIDsAdd, new List<string>());
        }

        public void ResetParts() {
            Parts = 0;
            PartsAutoGenerated = false;
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokes =
                from strokeID in PageObjectStrokeParentIDs
                from stroke in ParentPage.InkStrokes
                where (stroke.GetPropertyData(CLPPage.StrokeIDKey) as string) == strokeID
                select stroke;

            StrokeCollection inkStrokes = new StrokeCollection(strokes.ToList());
            return inkStrokes;
        }

        public void UpdatePartsFromHandwritingRegion() {
            HandwritingRegionParts.DoInterpretation();
            int num;
            bool success = int.TryParse(HandwritingRegionParts.StoredAnswer, out num);
            if (success)
            {
                Parts = num;
            }
            // Set back to null otherwise you may accidentally keep reading the old value
            HandwritingRegionParts.StoredAnswer = null;
            Console.WriteLine("After interpret Parts: " + Parts);
        }

        private void ClearHandWritingPartsStrokes() {
            Console.WriteLine("hw strokes : " + HandwritingRegionParts.GetStrokesOverPageObject().Count);
            foreach (Stroke stroke in HandwritingRegionParts.GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }
        }

        public bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            return StrokePathContainer.PageObjectIsOver(pageObject, .50);
        }

        public void AcceptObjects(List<string> addedPageObjectIDs, ObservableCollection<ICLPPageObject> removedPageObjects)
        {
            bool changed = false;
            if(CanAcceptPageObjects)
            {
                if (!PartsAutoGenerated) {
                    UpdatePartsFromHandwritingRegion();
                }

                var pageObjectsRemove =
                from pageObjectID in PageObjectObjectParentIDs
                from pageObject in removedPageObjects
                where (pageObject.UniqueID).Equals(pageObjectID)
                select pageObject;
                foreach(ICLPPageObject pageObject in pageObjectsRemove)
                {
                    changed = true;
                    Parts = (Parts-pageObject.Parts > 0) ? Parts - pageObject.Parts : 0;
                    PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
                }

                var pageObjectsAdd =
                    from pageObjectID in addedPageObjectIDs
                    from pageObject in ParentPage.PageObjects
                    where (pageObject.UniqueID).Equals(pageObjectID) && !pageObject.GetType().Equals(typeof(CLPStamp))
                    select pageObject;
                foreach (ICLPPageObject pageObject in pageObjectsAdd)
                {
                    changed = true;
                    Parts += pageObject.Parts;                   
                    PageObjectObjectParentIDs.Add(pageObject.UniqueID);
                }
                if (changed)
                {
                    PartsAutoGenerated = true;
                    ClearHandWritingPartsStrokes();
                }
            }
        }

        public ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects =
                from pageObjectID in PageObjectObjectParentIDs
                from pageObject in ParentPage.PageObjects
                where pageObject.UniqueID == pageObjectID
                select pageObject;

            ObservableCollection<ICLPPageObject> pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects.ToList());
            return pageObjectsOver;
        }

        #endregion //Methods

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage), null, includeInSerialization:false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentPageID
        {
            get
            {
                string tempValue = GetValue<string>(ParentPageIDProperty);
                if(tempValue != "")
                {
                    return tempValue;
                }
                else
                {
                    SetValue(ParentPageIDProperty, ParentPage.UniqueID);
                    return ParentPage.UniqueID;
                }
            }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentID
        {
            get { return GetValue<string>(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        public static readonly PropertyData ParentIDProperty = RegisterProperty("ParentID", typeof(string), "");

        /// <summary>
        /// Creation date of pageObject.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// UniqueID of pageObject.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> PageObjectStrokeParentIDs
        {
	        get { return GetValue<ObservableCollection<string>>(PageObjectStrokeParentIDsProperty); }
	        set { SetValue(PageObjectStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectStrokeParentIDsProperty = RegisterProperty("PageObjectStrokeParentIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());
        
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), true);

        /// <summary>
        /// UniqueIDs of the pageObjects above a pageObject.
        /// </summary>
        public ObservableCollection<string> PageObjectObjectParentIDs
        {
            get { return GetValue<ObservableCollection<string>>(PageObjectObjectParentIDsProperty); }
            set { SetValue(PageObjectObjectParentIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectObjectParentIDsProperty = RegisterProperty("PageObjectObjectParentIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof(bool), true);

        /// <summary>
        /// xPosition of pageObject on page, used for serialization.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set
            {
                SetValue(XPositionProperty, value);
                HandwritingRegionParts.XPosition = value;
                StrokePathContainer.XPosition = value;
            }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// YPosition of pageObject on page, used for serialization.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set
            {
                SetValue(YPositionProperty, value);
                HandwritingRegionParts.YPosition = value + Height - PARTS_SIDE;
                StrokePathContainer.YPosition = value + HANDLE_HEIGHT;
            }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 10.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set 
            { 
                SetValue(HeightProperty, value);
                StrokePathContainer.Height = Height - HANDLE_HEIGHT - PARTS_SIDE;
                if (StrokePathContainer.InternalPageObject != null)
                {
                    StrokePathContainer.InternalPageObject.Height = StrokePathContainer.Height;
                }
            }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 100);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set 
            { 
                SetValue(WidthProperty, value);
                StrokePathContainer.Width = Width;
                if (StrokePathContainer.InternalPageObject != null)
                {
                    StrokePathContainer.InternalPageObject.Width = StrokePathContainer.Width;
                }
            }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 100);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 0);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAdornersShow
        {
            get { return GetValue<bool>(CanAdornersShowProperty); }
            set { SetValue(CanAdornersShowProperty, value); }
        }

        /// <summary>
        /// Register the CanAdornersShow property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanAdornersShowProperty = RegisterProperty("CanAdornersShow", typeof(bool), true);
    }
}

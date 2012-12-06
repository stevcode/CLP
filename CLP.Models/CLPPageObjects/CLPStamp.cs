using System;
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

        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject, CLPPage page)
            : base()
        {
            ParentPage = page;
            ParentPageID = page.UniqueID;
            StrokePathContainer = new CLPStrokePathContainer(internalPageObject, page);

            Height = StrokePathContainer.Height + HANDLE_HEIGHT;
            Width = StrokePathContainer.Width;

            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            CanAcceptStrokes = true;
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

            return newStamp;
        }

        public void OnRemoved()
        {
            foreach (Stroke stroke in GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }
        }

        public virtual void RefreshStrokeParentIDs()
        {
            if(CanAcceptStrokes)
            {
                PageObjectStrokeParentIDs.Clear();

                Rect rect = new Rect(XPosition, YPosition, Width, Height);
                List<string> addedStrokeIDsOverObject = new List<string>();
                foreach(Stroke stroke in ParentPage.InkStrokes)
                {
                    if(stroke.HitTest(rect, 3))
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
                    PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }

            foreach(string strokeID in addedStrokes)
            {
                PageObjectStrokeParentIDs.Add(strokeID);
            }
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
        /// xPosition of pageObject on page, used for serialization.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// YPosition of pageObject on page, used for serialization.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 10.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value);
            StrokePathContainer.Height = Height - HANDLE_HEIGHT;
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
            set { SetValue(WidthProperty, value);
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
    }
}
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

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPStamp : DataObjectBase<CLPStamp>, ICLPPageObject
    {
        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject)
            : base()
        {
            StrokePathContainer = new CLPStrokePathContainer(internalPageObject);

            Position = new Point(100, 100);

            Height = StrokePathContainer.Height;
            Width = StrokePathContainer.Width;

            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            PageObjectStrokes = new ObservableCollection<string>();
            CanAcceptStrokes = true;
        }

        //Parameterless constructor for Protobuf
        private CLPStamp()
            : base()
        { }

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

        /// <summary>
        /// Register the InternalPageObject property so it is known in the class.
        /// </summary>
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

            return newStamp;
        }

        public void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
        }

        protected void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            StrokeCollection strokesToRemove = new StrokeCollection();
            StrokeCollection PageObjectActualStrokes = CLPPage.StringsToStrokes(PageObjectStrokes);
            foreach (Stroke objectStroke in PageObjectActualStrokes)
            {

                string objectStrokeUniqueID = objectStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                foreach (Stroke pageStroke in removedStrokes)
                {
                    string pageStrokeUniqueID = pageStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                    if (objectStrokeUniqueID == pageStrokeUniqueID)
                    {
                        strokesToRemove.Add(objectStroke);
                    }
                }
            }

            foreach (Stroke stroke in strokesToRemove)
            {
                string stringStroke = CLPPage.StrokeToString(stroke);
                PageObjectStrokes.Remove(stringStroke);
            }


            foreach (Stroke stroke in addedStrokes)
            {
                Stroke newStroke = stroke.Clone();
                Matrix transform = new Matrix();
                transform.Translate(-Position.X, -Position.Y - 50);
                newStroke.Transform(transform, true);

                PageObjectStrokes.Add(CLPPage.StrokeToString(newStroke));
            }
        }

        #endregion //Methods

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string PageID
        {
            get { return GetValue<string>(PageIDProperty); }
            set { SetValue(PageIDProperty, value); }
        }

        /// <summary>
        /// Register the PageID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageIDProperty = RegisterProperty("PageID", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentID
        {
            get { return GetValue<string>(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        /// <summary>
        /// Register the ParentID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ParentIDProperty = RegisterProperty("ParentID", typeof(string), "");

        /// <summary>
        /// Creation date of pageObject.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// UniqueID of pageObject.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// PageObject's personal collection of strokes.
        /// </summary>
        public ObservableCollection<string> PageObjectStrokes
        {
            get { return GetValue<ObservableCollection<string>>(PageObjectStrokesProperty); }
            set { SetValue(PageObjectStrokesProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectStrokesProperty = RegisterProperty("PageObjectStrokes", typeof(ObservableCollection<string>), new ObservableCollection<string>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        /// <summary>
        /// Register the CanAcceptStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), false);

        /// <summary>
        /// Position of pageObject on page.
        /// </summary>
        public Point Position
        {
            get { return GetValue<Point>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Register the Position property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(Point), new Point(10, 10));

        /// <summary>
        /// xPosition of pageObject on page, used for serialization.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        /// <summary>
        /// Register the XPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// YPosition of pageObject on page, used for serialization.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        /// <summary>
        /// Register the YPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 10.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value);
            StrokePathContainer.Height = Height - 50;
            Console.WriteLine("height changed");
            }
        }

        /// <summary>
        /// Register the Height property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 100);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value);
            StrokePathContainer.Width = Width;
            Console.WriteLine("width changed");
            }
        }

        /// <summary>
        /// Register the Width property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 100);
    }
}

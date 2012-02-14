using Catel.MVVM;
using System.Windows.Ink;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class CLPPageObjectBaseViewModel : ViewModelBase 
    {
        protected CLPPageObjectBaseViewModel() : base()
        {
        }

        public override string Title { get { return "APageObjectBaseVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
        public CLPPageObjectBase PageObject
        {
            get { return GetValue<CLPPageObjectBase>(PageObjectProperty); }
            protected set { SetValue(PageObjectProperty, value); }
        }

        /// <summary>
        /// Register the PageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(CLPPageObjectBase));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// Register the Height property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// Register the Width property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public Point Position
        {
            get { return GetValue<Point>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Register the Position property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(Point));
        
        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get { return _pageObjectStrokes; }
            protected set
            {
                _pageObjectStrokes = value;
            }
        }

        #endregion //Model

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedScribbles)
        {

        }

        protected virtual void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            StrokeCollection strokesToRemove = new StrokeCollection();
            foreach (Stroke objectStroke in PageObjectStrokes)
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
                PageObjectStrokes.Remove(stroke);

                string stringStroke = CLPPage.StrokeToString(stroke);
                PageObject.PageObjectStrokes.Remove(stringStroke);
            }


            foreach (Stroke stroke in addedStrokes)
            {
                Stroke newStroke = stroke.Clone();
                Matrix transform = new Matrix();
                transform.Translate(-Position.X, -Position.Y);
                newStroke.Transform(transform, true);
                PageObjectStrokes.Add(newStroke);

                PageObject.PageObjectStrokes.Add(CLPPage.StrokeToString(newStroke));
            }
        }
    }
}
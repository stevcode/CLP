using Catel.MVVM;
using System.Windows.Ink;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels.PageObjects;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class CLPPageObjectBaseViewModel : ViewModelBase 
    {
        public CLPPageViewModel PageViewModel { get; protected set; }

        protected CLPPageObjectBaseViewModel(CLPPageViewModel pageViewModel) : base()
        {
            PageViewModel = pageViewModel;
        }

        public override string Title { get { return "APageObjectBaseVM"; } }

        private CLPPageObjectBase _pageObject;
        public CLPPageObjectBase PageObject
        {
            get
            {
                return _pageObject;
            }
            set
            {
                _pageObject = value;
                this.Position = _pageObject.Position;
                this.Height = _pageObject.Height;
                this.Width = _pageObject.Width;
            }
        }

        private PageObjectContainerViewModel _container;
        public PageObjectContainerViewModel Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
            }
        }
        
        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get { return _pageObjectStrokes; }
            protected set
            {
                _pageObjectStrokes = value;
            }
        }

        #region Bindings

        /// <summary>
        /// The <see cref="Height" /> property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        private double _height = 0;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                _height = value;
                RaisePropertyChanged(HeightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Width" /> property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        private double _width = 0;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
                RaisePropertyChanged(WidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Position" /> property's name.
        /// </summary>
        public const string PositionPropertyName = "Position";

        private Point _position = new Point(0,0);

        /// <summary>
        /// Sets and gets the Position property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Point Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                RaisePropertyChanged(PositionPropertyName);
            }
        }

        #endregion //Bindings

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
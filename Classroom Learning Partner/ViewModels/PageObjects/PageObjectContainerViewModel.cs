using System.Windows;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class PageObjectContainerViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the PageObjectContainerViewModel class.
        /// </summary>
        public PageObjectContainerViewModel(CLPPageObjectBaseViewModel pageObjectBaseViewModel) : base()
        {
            Position = pageObjectBaseViewModel.Position;
            Width = pageObjectBaseViewModel.Width;
            Height = pageObjectBaseViewModel.Height;
            Visible = Visibility.Visible;

            if (pageObjectBaseViewModel is CLPImageViewModel)
            {
                _pageObjectViewModel = pageObjectBaseViewModel as CLPImageViewModel;
            }
            else if (pageObjectBaseViewModel is CLPImageStampViewModel)
            {
                _pageObjectViewModel = pageObjectBaseViewModel as CLPImageStampViewModel;
            }
            else if (pageObjectBaseViewModel is CLPBlankStampViewModel)
            {
                _pageObjectViewModel = pageObjectBaseViewModel as CLPBlankStampViewModel;
            }
            else if (pageObjectBaseViewModel is CLPTextBoxViewModel)
            {
                _pageObjectViewModel = pageObjectBaseViewModel as CLPTextBoxViewModel;
            }
            else if (pageObjectBaseViewModel is CLPSnapTileContainerViewModel)
            {
                _pageObjectViewModel = pageObjectBaseViewModel as CLPSnapTileContainerViewModel;
            }

            //Register with messages to turn invisible to start playback
            playbackOn = false;
           /* AppMessages.ChangePlayback.Register(this, (playback) =>
            {
                playbackOn = !playbackOn;
                if (!playbackOn)
                    this.Visible = Visibility.Visible;
                else
                    this.Visible = Visibility.Collapsed;
               //RaisePropertyChanged("Visible");
                
            }); */
        }
        private bool playbackOn;
        private CLPPageObjectBaseViewModel _pageObjectViewModel;
        public CLPPageObjectBaseViewModel PageObjectViewModel
        {
            get
            {
                return _pageObjectViewModel;
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
                if (PageObjectViewModel != null)
                {
                    PageObjectViewModel.Width = _width;
                }     
            }
        }

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
                if (PageObjectViewModel != null)
                {
                    PageObjectViewModel.Height = _height;
                }
            }
        }
        private Visibility _visible;
        public Visibility Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                RaisePropertyChanged("Visible");
            }
        }
    }
}
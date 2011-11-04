using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPImageStampViewModel : CLPPageObjectBaseViewModel
    {
        #region Constructors

        public CLPImageStampViewModel(CLPImageStamp stamp)
        {
            this.Position = stamp.Position;
            /* Change stamp handle dimensions here */

            Double handle_height = 50;
            Double handle_upper_width = 100;
            Double handle_lower_width = 80;

            /* End of stamp handle dimensions */

            Double handle_width = Math.Max(handle_upper_width, handle_lower_width);
            _points = (handle_width / 2 - handle_upper_width / 2).ToString() + " 0 " + (handle_width / 2 + handle_upper_width / 2).ToString() + " 0 " + (handle_width / 2 + handle_lower_width / 2).ToString() + " " + handle_height + " " + (handle_width / 2 - handle_lower_width / 2).ToString() + " " + handle_height;

            _sourceImage = stamp.SourceImage;
            _isAnchor = stamp.IsAnchor;
            if (!_isAnchor)
            {
                HandleVisibility = Visibility.Collapsed;
                BorderBrush = System.Windows.Media.Brushes.Transparent;
            }
            _borderBrush = System.Windows.Media.Brushes.Black;
        }

        #endregion //Constructors

        #region Bindings

        private ImageSource _sourceImage;

        /// <summary>
        /// Sets and gets the SourceImage property.
        /// </summary>
        public ImageSource SourceImage
        {
            get
            {
                return _sourceImage;
            }
        }

        /// <summary>
        /// The <see cref="IsAnchor" /> property's name.
        /// </summary>
        public const string IsAnchorPropertyName = "IsAnchor";

        private bool _isAnchor = true;

        /// <summary>
        /// Sets and gets the IsAnchor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAnchor
        {
            get
            {
                return _isAnchor;
            }

            set
            {
                if (_isAnchor == value)
                {
                    return;
                }

                _isAnchor = value;
                RaisePropertyChanged(IsAnchorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Points" /> property's name.
        /// </summary>
        public const string PointsPropertyName = "Points";

        private string _points = "";

        /// <summary>
        /// Sets and gets the Points property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Points
        {
            get
            {
                return _points;
            }

            set
            {
                if (_points == value)
                {
                    return;
                }

                _points = value;
                RaisePropertyChanged(PointsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HandleVisibility" /> property's name.
        /// </summary>
        public const string HandleVisibilityPropertyName = "HandleVisibility";

        private Visibility _handleVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the HandleVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility HandleVisibility
        {
            get
            {
                return _handleVisibility;
            }

            set
            {
                if (_handleVisibility == value)
                {
                    return;
                }

                _handleVisibility = value;
                RaisePropertyChanged(HandleVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BorderBrush" /> property's name.
        /// </summary>
        public const string BorderBrushPropertyName = "BorderBrush";

        private SolidColorBrush _borderBrush;

        /// <summary>
        /// Sets and gets the BorderBrush property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush BorderBrush
        {
            get
            {
                return _borderBrush;
            }

            set
            {
                if (_borderBrush == value)
                {
                    return;
                }

                _borderBrush = value;
                RaisePropertyChanged(BorderBrushPropertyName);
            }
        }

        #endregion //Bindings
    }
}

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
            PageObject = stamp;
            this.Position = stamp.Position;

            _sourceImage = stamp.SourceImage;
            _isAnchored = stamp.IsAnchored;
            if (!_isAnchored)
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
        public const string IsAnchorPropertyName = "IsAnchored";

        private bool _isAnchored = true;

        /// <summary>
        /// Sets and gets the IsAnchor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAnchored
        {
            get
            {
                return _isAnchored;
            }

            set
            {
                if (_isAnchored == value)
                {
                    return;
                }

                _isAnchored = value;
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

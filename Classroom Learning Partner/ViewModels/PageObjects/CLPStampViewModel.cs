using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPStampViewModel : CLPPageObjectBaseViewModel
    {
        #region Constructors

        public CLPStampViewModel(CLPImageStamp stamp)
        {
            PageObject = stamp;
            this.Position = stamp.Position;

            _sourceImage = stamp.SourceImage;
            _isAnchored = stamp.IsAnchored;
            _parts = stamp.Parts;
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
        /// The <see cref="StampParts" /> property's name.
        /// </summary>
        public const string PartsPropertyName = "StampParts";

        private string _parts = "";

        /// <summary>
        /// Sets and gets the StampParts property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Parts
        {
            get
            {
                return _parts;
            }

            set
            {
                _parts = value;
                RaisePropertyChanged(PartsPropertyName);
            }
        }

        #endregion //Bindings
    }
}

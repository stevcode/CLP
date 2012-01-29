using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Media;
using Classroom_Learning_Partner.Model;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPImageStampViewModel : CLPStampBaseViewModel
    {
        #region Constructors

        public CLPImageStampViewModel(CLPImageStamp stamp, CLPPageViewModel pageViewModel)
            : base(stamp, pageViewModel)
        {
            _isAnchored = stamp.IsAnchored;
            _parts = stamp.Parts;
            _sourceImage = stamp.SourceImage;
            PageObject = stamp;           
        }

        #endregion //Constructors

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

        #region Bindings



        /// <summary>
        /// The <see cref="Parts" /> property's name.
        /// </summary>
        public const string PartsPropertyName = "Parts";

        private int _parts = 0;

        /// <summary>
        /// Sets and gets the Parts property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Parts
        {
            get
            {
                return _parts;
            }

            set
            {
                if (_parts == value)
                {
                    return;
                }

                _parts = value;
                //bad, quick hack, update to set database values
                (PageObject as CLPImageStamp).Parts = value;
                RaisePropertyChanged(PartsPropertyName);
            }
        }

        #endregion //Bindings
    }
}

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
            SourceImage = stamp.SourceImage;
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
            set 
            {
                _sourceImage = value;
            }
        }

        #endregion //Bindings
    }
}

using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class CLPImageViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            PageObject = image;
            _sourceImage = image.SourceImage;
            
        }

        #region Binding

        /// <summary>
        /// The <see cref="SourceImage" /> property's name.
        /// </summary>
        public const string SourceImagePropertyName = "SourceImage";

        private ImageSource _sourceImage;

        /// <summary>
        /// Sets and gets the SourceImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ImageSource SourceImage
        {
            get
            {
                return _sourceImage;
            }

            set
            {
                if (_sourceImage == value)
                {
                    return;
                }

                _sourceImage = value;
                RaisePropertyChanged(SourceImagePropertyName);
            }
        }

        #endregion //Binding
    }
}
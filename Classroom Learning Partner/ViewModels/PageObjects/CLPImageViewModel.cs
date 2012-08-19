using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPImageViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image) : base()
        {
            PageObject = image;
        }

        public override string Title { get { return "ImageVM"; } }

        #region Binding

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        /// <summary>
        /// Register the SourceImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof(ImageSource));

        #endregion //Binding
    }
}
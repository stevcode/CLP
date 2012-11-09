using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(MainWindowViewModel))]
    public class CLPImageViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image) : base()
        {
            PageObject = image;
            //CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);
            List<byte> ByteSource = PageObject.ParentPage.ImagePool[image.ImageID];
            LoadImageFromByteSource(ByteSource.ToArray());
        }

        public override string Title { get { return "ImageVM"; } }

        #region Binding

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        /// <summary>
        /// Register the SourceImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof(ImageSource), null);

        #endregion //Binding

        public void LoadImageFromByteSource(byte[] byteSource)
        {
            MemoryStream memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            BitmapImage genBmpImage = new BitmapImage();

            genBmpImage.BeginInit();
            genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
            //genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            genBmpImage.StreamSource = memoryStream;
            genBmpImage.EndInit();
            genBmpImage.Freeze();

            memoryStream.Dispose();
            memoryStream = null;

            SourceImage = genBmpImage;
        }
    }
}
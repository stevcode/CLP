using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPImageViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image)
        {
            PageObject = image;
            try
            {
                var byteSource = image.ParentPage.ImagePool[image.ImageID];
                LoadImageFromByteSource(byteSource.ToArray());
            }
            catch (System.Exception ex)
            {
                Logger.Instance.WriteToLog(
                    "ImageVM failed to load Image from ByteSource, image.ParentPage likely null. Error: " + ex.Message);
            }

            double aspectRatio = 1.0;
            if (SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Width/SourceImage.Height;
            }
           // PageObject.EnforceAspectRatio(aspectRatio);

            ResizeImageCommand = new Command<DragDeltaEventArgs>(OnResizeImageCommandExecute);
        }

        public override string Title
        {
            get { return "ImageVM"; }
        }

        #region Binding

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof (ImageSource),
            null);

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

        /// <summary>
        /// Gets the CLPImageResize command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeImageCommand { get; set; }

        /// <summary>
        /// Method to invoke when the ResizeImageCommand command is executed.
        /// </summary>
        private void OnResizeImageCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage =
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(
                    PageObject.ParentPageID);

            PageObject.Height = PageObject.Height + e.VerticalChange;
            PageObject.Width = PageObject.Width + e.HorizontalChange;
            if(PageObject.Height < 10)
            {
                PageObject.Height = 10;
            }
            if(PageObject.Width < 10)
            {
                PageObject.Width = 10;
            }
            if(PageObject.Height + PageObject.YPosition > parentPage.PageHeight)
            {
                PageObject.Height = PageObject.Height;
            }
            if(PageObject.Width + PageObject.XPosition > parentPage.PageWidth)
            {
                PageObject.Width = PageObject.Width;
            }

            double aspectRatio = 1.0;
            if(SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Width / SourceImage.Height;
            }
            PageObject.EnforceAspectRatio(aspectRatio);

            ChangePageObjectDimensions(PageObject, PageObject.Height, PageObject.Width);
        }
    }
}
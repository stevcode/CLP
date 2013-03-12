using System.Collections.Generic;
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
            : base()
        {
            PageObject = image;
            //CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);
            try
            {
                List<byte> ByteSource = image.ParentPage.ImagePool[image.ImageID];
                LoadImageFromByteSource(ByteSource.ToArray());
            }
            catch(System.Exception ex)
            {
                Logger.Instance.WriteToLog("ImageVM failed to load Image from ByteSource, image.ParentPage likely null. Error: " + ex.Message);
            }

            double aspectRatio = 1.0;
            if(SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Height/SourceImage.Width;
            }
            double newHeight, newWidth;
            if(PageObject.Height > PageObject.Width)
            {
                newHeight = PageObject.Height;
                newWidth = newHeight / aspectRatio;
            }
            else
            {
                newWidth = PageObject.Width;
                newHeight = aspectRatio * newWidth;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);

            ResizeImageCommand = new Command<DragDeltaEventArgs>(OnResizeImageCommandExecute);
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

        /// <summary>
        /// Gets the CLPImageResize command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeImageCommand { get; set; }

        /// <summary>
        /// Method to invoke when the ResizeImageCommand command is executed.
        /// </summary>
        private void OnResizeImageCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double aspectRatio = 1.0;
            if(SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Height/SourceImage.Width;
            }
            double newHeight, newWidth;
            if(e.VerticalChange > e.HorizontalChange)
            {
                newHeight = PageObject.Height + e.VerticalChange;
                if(newHeight < 10)
                {
                    newHeight = 10;
                }
                newWidth = newHeight / aspectRatio;
            }
            else
            {
                newWidth = PageObject.Width + e.HorizontalChange;
                if(newWidth < 10)
                {
                    newWidth = 10;
                }
                newHeight = aspectRatio * newWidth;
            }

            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
                newWidth = newHeight / aspectRatio;
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
                newHeight = aspectRatio * newWidth;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }
            }
        }
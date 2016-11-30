using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPImageViewModel : APageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image, INotebookService notebookService)
        {
            PageObject = image;
            if(App.MainWindowViewModel.ImagePool.ContainsKey(image.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[image.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(notebookService.CurrentImageCacheDirectory);
                foreach(var imageFilePath in from imageFilePath in imageFilePaths
                                             let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
                                             where imageHashID == image.ImageHashID
                                             select imageFilePath) 
                                             {
                                                 filePath = imageFilePath;
                                                 break;
                                             }

                var bitmapImage = CLPImage.GetImageFromPath(filePath);
                if(bitmapImage != null)
                {
                    SourceImage = bitmapImage;
                    App.MainWindowViewModel.ImagePool.Add(image.ImageHashID, bitmapImage);
                }
            }

            ResizeImageCommand = new Command<DragDeltaEventArgs>(OnResizeImageCommandExecute);
        }

        public override string Title { get { return "ImageVM"; } }

        #region Binding

        /// <summary>
        /// The visible image, loaded from the ImageCache.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof (ImageSource));

        #endregion //Binding

        /// <summary>
        /// Gets the CLPImageResize command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeImageCommand { get; set; }

        private void OnResizeImageCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

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
            if(PageObject.Height + PageObject.YPosition > parentPage.Height)
            {
                PageObject.Height = PageObject.Height;
            }
            if(PageObject.Width + PageObject.XPosition > parentPage.Width)
            {
                PageObject.Width = PageObject.Width;
            }

            var aspectRatio = 1.0;
            if(SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Width / SourceImage.Height;
            }
            //PageObject.EnforceAspectRatio(aspectRatio);

            ChangePageObjectDimensions(PageObject, PageObject.Height, PageObject.Width);
        }

        #region Static Methods

        public static BitmapImage LoadImageFromByteSource(byte[] byteSource)
        {
            var memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            var genBmpImage = new BitmapImage();

            genBmpImage.BeginInit();
            genBmpImage.CacheOption = BitmapCacheOption.OnDemand;
            //genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            genBmpImage.StreamSource = memoryStream;
            genBmpImage.EndInit();
            genBmpImage.Freeze();

            memoryStream.Dispose();

            return genBmpImage;
        }

        public static void AddImageToPage(CLPPage page)
        {
            // Configure open file dialog box
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"    // Filter files by extension
            };

            var result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }

            // Open document
            var filename = dlg.FileName;
            if (File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);

                var md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(bytes);
                var imageHashID = Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Replace("=", "");
                var newFileName = imageHashID + Path.GetExtension(filename);
                var newFilePath = Path.Combine(Catel.IoC.ServiceLocator.Default.ResolveType<INotebookService>().CurrentImageCacheDirectory, newFileName);

                try
                {
                    File.Copy(filename, newFilePath);
                }
                catch (IOException)
                {
                    MessageBox.Show("Image already in ImagePool, using ImagePool instead.");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong copying the image to the ImagePool. See Error Log.");
                    Logger.Instance.WriteToLog("[IMAGEPOOL ERROR]: " + e.Message);
                    return;
                }

                var bitmapImage = CLPImage.GetImageFromPath(newFilePath);
                if (bitmapImage == null)
                {
                    MessageBox.Show("Failed to load image from ImageCache by fileName.");
                    return;
                }

                if (!App.MainWindowViewModel.ImagePool.ContainsKey(imageHashID))
                {
                    App.MainWindowViewModel.ImagePool.Add(imageHashID, bitmapImage);
                }

                var visualImage = System.Drawing.Image.FromFile(newFilePath);
                var image = new CLPImage(page, imageHashID, visualImage.Height, visualImage.Width);

                ACLPPageBaseViewModel.AddPageObjectToPage(image);
            }
            else
            {
                MessageBox.Show("Error opening image file. Please try again.");
            }
        }

        #endregion //Static Methods
    }
}
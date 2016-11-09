using System;
using System.Collections.Generic;
using System.Drawing;
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
using Catel.Services;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;


namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPImageViewModel : APageObjectBaseViewModel
    {
        #region Fields

        private readonly IDataService _dataService;

        #endregion // Fields

        #region Constructor

        /// <summary>Initializes a new instance of the CLPImageViewModel class.</summary>
        public CLPImageViewModel(CLPImage image, IDataService dataService)
        {
            PageObject = image;
            _dataService = dataService;
            SourceImage = _dataService.GetImage(image.ImageHashID, image);

            InitializeCommands();
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Make Copies", "pack://application:,,,/Resources/Images/AddToDisplay.png", CreateImageCopyCommand, null, true));
        }

        #endregion // Constructor

        #region Binding

        /// <summary>The visible image, loaded from the ImageCache.</summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof(ImageSource));

        #endregion //Binding

        #region Commands

        private void InitializeCommands()
        {
            ResizeImageCommand = new Command<DragDeltaEventArgs>(OnResizeImageCommandExecute);
            CreateImageCopyCommand = new Command(OnCreateImageCopyCommandExecute);
        }

        /// <summary>Resizes the image while keeping aspect ratio.</summary>
        public Command<DragDeltaEventArgs> ResizeImageCommand { get; set; }

        private void OnResizeImageCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

            PageObject.Height = PageObject.Height + e.VerticalChange;
            PageObject.Width = PageObject.Width + e.HorizontalChange;
            if (PageObject.Height < 10)
            {
                PageObject.Height = 10;
            }
            if (PageObject.Width < 10)
            {
                PageObject.Width = 10;
            }
            if (PageObject.Height + PageObject.YPosition > parentPage.Height)
            {
                PageObject.Height = PageObject.Height;
            }
            if (PageObject.Width + PageObject.XPosition > parentPage.Width)
            {
                PageObject.Width = PageObject.Width;
            }

            var aspectRatio = 1.0;
            if (SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Width / SourceImage.Height;
            }
            //PageObject.EnforceAspectRatio(aspectRatio);

            ChangePageObjectDimensions(PageObject, PageObject.Height, PageObject.Width);
        }

        /// <summary>SUMMARY</summary>
        public Command CreateImageCopyCommand { get; private set; }

        private void OnCreateImageCopyCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfImages = Int32.Parse(keyPad.NumbersEntered.Text);

            var xPosition = 10.0;
            var yPosition = 160.0;
            if (YPosition + 2 * Height + 10.0 < PageObject.ParentPage.Height)
            {
                yPosition = YPosition + Height + 10.0;
            }
            else if (XPosition + 2 * Width + 10.0 < PageObject.ParentPage.Width)
            {
                yPosition = YPosition;
                xPosition = XPosition + Width + 10.0;
            }

            var imagesToAdd = new List<CLPImage>();
            foreach (var index in Enumerable.Range(1, numberOfImages))
            {
                var image = PageObject.Duplicate() as CLPImage;
                if (image == null)
                {
                    continue;
                }
                image.XPosition = xPosition;
                image.YPosition = yPosition;

                if (xPosition + 2 * image.Width <= PageObject.ParentPage.Width)
                {
                    xPosition += image.Width;
                }
                //If there isn't room, diagonally pile the rest
                else if ((xPosition + image.Width + 20.0 <= PageObject.ParentPage.Width) &&
                         (yPosition + image.Height + 20.0 <= PageObject.ParentPage.Height))
                {
                    xPosition += 20.0;
                    yPosition += 20.0;
                }
                imagesToAdd.Add(image);
            }

            if (imagesToAdd.Count == 1)
            {
                ACLPPageBaseViewModel.AddPageObjectToPage(imagesToAdd.First());
            }
            else
            {
                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, imagesToAdd);
            }
        }

        #endregion //Commands

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
            var dependencyResolver = ServiceLocator.Default.GetDependencyResolver();
            var openFileService = dependencyResolver.Resolve<IOpenFileService>();
            openFileService.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif";
            openFileService.IsMultiSelect = false;
            if (!openFileService.DetermineFile())
            {
                return;
            }

            var imageFilePath = openFileService.FileName;

            Image visualImage;
            try
            {
                visualImage = Image.FromFile(imageFilePath);
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening image file. Please try again.");
                return;
            }

            var dataService = dependencyResolver.Resolve<IDataService>();
            var imageHashID = dataService.SaveImageToImagePool(imageFilePath, page);

            if (string.IsNullOrWhiteSpace(imageHashID))
            {
                return;
            }

            var image = new CLPImage(page, imageHashID, visualImage.Height, visualImage.Width);
            ACLPPageBaseViewModel.AddPageObjectToPage(image);
        }

        #endregion //Static Methods
    }
}
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Controls.WebcamPlayer;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class WebcamPanelViewModel : APanelBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebcamPanelViewModel" /> class.
        /// </summary>
        public WebcamPanelViewModel()
        {
            Location = PanelLocations.Right;
            Length = InitialLength;
            SelectedWebcam = new CapDevice("");
            SelectedWebcam.MonikerString = CapDevice.DeviceMonikers[0].MonikerString;

            foreach(FilterInfo device in CapDevice.DeviceMonikers)
            {
                if(device.Name.ToUpper().Contains("V"))
                {
                    SelectedWebcam.MonikerString = device.MonikerString;
                    break;
                }
            }

            CaptureImageCommand = new Command<CapPlayer>(OnCaptureImageCommandExecute);
            AddImageCommand = new Command(OnAddImageCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "WebcamPanelVM"; }
        }

        protected override async Task CloseAsync()
        {
            SelectedWebcam.Dispose();
            SelectedWebcam = null;

            await base.CloseAsync();
        }

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CapDevice SelectedWebcam
        {
            get { return GetValue<CapDevice>(SelectedWebcamProperty); }
            set { SetValue(SelectedWebcamProperty, value); }
        }

        public static readonly PropertyData SelectedWebcamProperty = RegisterProperty("SelectedWebcam", typeof(CapDevice), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double WebcamRotation
        {
            get { return GetValue<double>(WebcamRotationProperty); }
            set { SetValue(WebcamRotationProperty, value); }
        }

        public static readonly PropertyData WebcamRotationProperty = RegisterProperty("WebcamRotation", typeof(double), 180d);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<BitmapSource> CapturedImages
        {
            get { return GetValue<ObservableCollection<BitmapSource>>(CapturedImagesProperty); }
            set { SetValue(CapturedImagesProperty, value); }
        }

        public static readonly PropertyData CapturedImagesProperty = RegisterProperty("CapturedImages", typeof(ObservableCollection<BitmapSource>), () => new ObservableCollection<BitmapSource>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BitmapSource SelectedImage
        {
            get { return GetValue<BitmapSource>(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly PropertyData SelectedImageProperty = RegisterProperty("SelectedImage", typeof(BitmapSource), null);

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Gets the CaptureImageCommand command.
        /// </summary>
        public Command<CapPlayer> CaptureImageCommand { get; private set; }

        private void OnCaptureImageCommandExecute(CapPlayer webcamPlayer)
        {
            //// Store current image in the webcam
            var bitmap = webcamPlayer.CurrentBitmap;
            if(bitmap != null)
            {
                CapturedImages.Insert(0, bitmap);
                SelectedImage = CapturedImages[0];
            }
        }

        /// <summary>
        /// Gets the AddImageCommand command.
        /// </summary>
        public Command AddImageCommand { get; private set; }

        private void OnAddImageCommandExecute()
        {
            // TODO: Entities
            //var currentPage = ((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage;

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(SelectedImage));
            //encoder.QualityLevel = 100;
            //byte[] byteSource = new byte[0];
            //using (MemoryStream stream = new MemoryStream())
            //{               
            //    encoder.Frames.Add(BitmapFrame.Create(SelectedImage));
            //    encoder.Save(stream);
            //    byteSource = stream.ToArray(); 
            //    stream.Close();               
            //}

            //List<byte> ByteSource = new List<byte>(byteSource);

            //MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //byte[] hash = md5.ComputeHash(byteSource);
            //string imageID = Convert.ToBase64String(hash);

            //if(!currentPage.ImagePool.ContainsKey(imageID))
            //{
            //    currentPage.ImagePool.Add(imageID, ByteSource);
            //}
            //CLPImage image = new CLPImage(imageID, currentPage, SelectedImage.Height, SelectedImage.Width);

            ////TODO: Steve - All this is a hack for science webcam usage. Fix to be generalized.
            //int pageObjectIndex = -1;
            //if (currentPage.PageIndex == 25)
            //{
            //    foreach(ICLPPageObject pageObject in currentPage.PageObjects)
            //    {
            //        if(pageObject is CLPImage && pageObject.YPosition == 225 && pageObject.XPosition == 108)
            //        {
            //            pageObjectIndex = currentPage.PageObjects.IndexOf(pageObject);
            //            break;
            //        }
            //    }

            //    if(pageObjectIndex >= 0)
            //    {
            //        currentPage.PageObjects.RemoveAt(pageObjectIndex);
            //    }

            //    ACLPPageBaseViewModel.AddPageObjectToPage(image);
            //    image.IsBackground = true;
            //    image.Height = 450;
            //    image.Width = 600;
            //    image.YPosition = 225;
            //    image.XPosition = 108;
            //}
        }

        #endregion //Commands
    }
}
using CLP.Models;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Controls.WebcamPlayer;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.IO;

namespace Classroom_Learning_Partner.ViewModels
{

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class WebcamPanelViewModel : ViewModelBase, IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebcamPanelViewModel"/> class.
        /// </summary>
        public WebcamPanelViewModel()
        {
            SelectedWebcam = new CapDevice("");
            SelectedWebcam.MonikerString = CapDevice.DeviceMonikers[0].MonikerString;

            foreach(var device in CapDevice.DeviceMonikers)
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
        public override string Title { get { return "WebcamPanelVM"; } }

        protected override void Close()
        {
            SelectedWebcam.Dispose();
            SelectedWebcam = null;

            base.Close();
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
            BitmapSource bitmap = webcamPlayer.CurrentBitmap;
            if(bitmap != null)
            {
                CapturedImages.Insert(0,bitmap);
                SelectedImage = CapturedImages[0];
            }
        }

        /// <summary>
        /// Gets the AddImageCommand command.
        /// </summary>
        public Command AddImageCommand { get; private set; }

        private void OnAddImageCommandExecute()
        {
            var currentPage = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(SelectedImage));
            encoder.QualityLevel = 100;
            byte[] byteSource = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {               
                encoder.Frames.Add(BitmapFrame.Create(SelectedImage));
                encoder.Save(stream);
                byteSource = stream.ToArray(); 
                stream.Close();               
            }

            List<byte> ByteSource = new List<byte>(byteSource);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(byteSource);
            string imageID = Convert.ToBase64String(hash);

            if(!currentPage.ImagePool.ContainsKey(imageID))
            {
                currentPage.ImagePool.Add(imageID, ByteSource);
            }
            CLPImage image = new CLPImage(imageID, currentPage);

            //TODO: Steve - All this is a hack for science webcam usage. Fix to be generalized.
            int pageObjectIndex = -1;
            if (currentPage.PageIndex == 25)
            {
                foreach(ICLPPageObject pageObject in currentPage.PageObjects)
                {
                    if(pageObject is CLPImage && pageObject.YPosition == 225 && pageObject.XPosition == 108)
                    {
                        pageObjectIndex = currentPage.PageObjects.IndexOf(pageObject);
                        break;
                    }
                }

                if(pageObjectIndex >= 0)
                {
                    currentPage.PageObjects.RemoveAt(pageObjectIndex);
                }

                CLPServiceAgent.Instance.AddPageObjectToPage(image);
                image.IsBackground = true;
                image.Height = 450;
                image.Width = 600;
                image.YPosition = 225;
                image.XPosition = 108;
            }
        }

        #endregion //Commands

        #region IPanel Members

        public string PanelName
        {
            get
            {
                return "WebcamPanel";
            }
        }

        /// <summary>
        /// Whether the Panel is pinned to the same Z-Index as the Workspace.
        /// </summary>
        public bool IsPinned
        {
            get { return GetValue<bool>(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        public static readonly PropertyData IsPinnedProperty = RegisterProperty("IsPinned", typeof(bool), true);

        /// <summary>
        /// Visibility of Panel, True for Visible, False for Collapsed.
        /// </summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), true);

        /// <summary>
        /// Can the Panel be resized.
        /// </summary>
        public bool IsResizable
        {
            get { return GetValue<bool>(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), false);

        /// <summary>
        /// Initial Width of the Panel, before any resizing.
        /// </summary>
        public double InitialWidth
        {
            get { return 250; }
        }

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Right);

        /// <summary>
        /// A Linked IPanel if more than one IPanel is to be used in the same Location.
        /// </summary>
        public IPanel LinkedPanel
        {
            get { return GetValue<IPanel>(LinkedPanelProperty); }
            set { SetValue(LinkedPanelProperty, value); }
        }

        public static readonly PropertyData LinkedPanelProperty = RegisterProperty("LinkedPanel", typeof(IPanel), null);

        #endregion
    }
}

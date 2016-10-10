using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStampCopyViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPStampCopyViewModel"/> class.
        ///// </summary>
        //public CLPStampCopyViewModel(CLPStampCopy container)
        //{
        //    PageObject = container;

        //    if(container.ImageID != string.Empty)
        //    {
        //        try
        //        {
        //            var byteSource = container.ParentPage.ImagePool[container.ImageID];
        //            LoadImageFromByteSource(byteSource.ToArray());
        //        }
        //        catch(Exception ex)
        //        {
        //            Logger.Instance.WriteToLog("ImageVM failed to load Image from ByteSource, container.ParentPage likely null. Error: " + ex.Message);
        //        }
        //    }

        //    if(container.IsStamped)
        //    {
        //        ScribblesToStrokePaths();
        //    }
        //}

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StampCopyVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Whether the stamp copy has been copied to the page.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsStamped
        {
            get { return GetValue<bool>(IsStampedProperty); }
            set { SetValue(IsStampedProperty, value); }
        }

        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool), propertyChangedEventHandler: IsStamped_Changed);

        private static void IsStamped_Changed(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var stampCopyViewModel = sender as CLPStampCopyViewModel;
            if(stampCopyViewModel == null || !stampCopyViewModel.IsStamped)
            {
                return;
            }

            stampCopyViewModel.ScribblesToStrokePaths();
        }

        /// <summary>
        /// Whether or not the StampCopy is a copy of a Collection Stamp.
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsCollectionCopy
        {
            get { return GetValue<bool>(IsCollectionCopyProperty); }
            set { SetValue(IsCollectionCopyProperty, value); }
        }

        public static readonly PropertyData IsCollectionCopyProperty = RegisterProperty("IsCollectionCopy", typeof(bool));

        /// <summary>
        /// Number of parts represented by the StampCopy. Only visible for collection copies.
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int));

        #endregion //Model

        #region Binding

        /// <summary>
        /// The visible image, loaded from the page's ImagePool.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof(ImageSource));

        private readonly ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        {
            get
            {
                return _strokePathViewModels;
            }
        }

        #endregion //Binding

        #region Methods

        private void LoadImageFromByteSource(byte[] byteSource)
        {
            var memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            var genBmpImage = new BitmapImage();

            genBmpImage.BeginInit();
            genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
            //genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            genBmpImage.StreamSource = memoryStream;
            genBmpImage.EndInit();
            genBmpImage.Freeze();

            memoryStream.Dispose();

            SourceImage = genBmpImage;
        }

        private void ScribblesToStrokePaths()
        {
            //var clpStampCopy = PageObject as CLPStampCopy;
            //if(clpStampCopy == null)
            //{
            //    return;
            //}
            //StrokePathViewModels.Clear();

            //var inkStrokes = StrokeDTO.LoadInkStrokes(clpStampCopy.SerializedStrokes);

            //foreach (var stroke in inkStrokes)
            //{
            //    var firstPoint = stroke.StylusPoints[0];

            //    var geometry = new StreamGeometry();
            //    using (var geometryContext = geometry.Open())
            //    {
            //        geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
            //        foreach (var point in stroke.StylusPoints)
            //        {
            //            geometryContext.LineTo(new Point(point.X, point.Y), true, true);
            //        }
            //    }
            //    geometry.Freeze();

            //    var strokePathViewModel = new StrokePathViewModel(geometry, 
            //                                                      (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString()), 
            //                                                      stroke.DrawingAttributes.Width,
            //                                                      stroke.DrawingAttributes.IsHighlighter);
            //    StrokePathViewModels.Add(strokePathViewModel);
            //}
        }

        #endregion //Methods
    }
}

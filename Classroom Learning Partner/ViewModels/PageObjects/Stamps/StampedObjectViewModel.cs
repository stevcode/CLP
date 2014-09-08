using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class StampedObjectViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StampedObjectViewModel" /> class.
        /// </summary>
        public StampedObjectViewModel(StampedObject stampedObject)
        {
            PageObject = stampedObject;
            if(App.MainWindowViewModel.ImagePool.ContainsKey(stampedObject.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[stampedObject.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(MainWindowViewModel.ImageCacheDirectory);
                foreach(var imageFilePath in from imageFilePath in imageFilePaths
                                             let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
                                             where imageHashID == stampedObject.ImageHashID
                                             select imageFilePath)
                {
                    filePath = imageFilePath;
                    break;
                }

                var bitmapImage = CLPImage.GetImageFromPath(filePath);
                if(bitmapImage != null)
                {
                    SourceImage = bitmapImage;
                    App.MainWindowViewModel.ImagePool.Add(stampedObject.ImageHashID, bitmapImage);
                }
            }

            foreach(var strokePath in stampedObject.SerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke()).Select(stroke => new StrokePathDTO(stroke))) 
            {
                StrokePaths.Add(strokePath);
            }
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "StampedObjectVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Whether or not the StampCopy is a copy of a Collection Stamp.
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsStampedCollection
        {
            get { return GetValue<bool>(IsStampedCollectionProperty); }
            set { SetValue(IsStampedCollectionProperty, value); }
        }

        public static readonly PropertyData IsStampedCollectionProperty = RegisterProperty("IsStampedCollection", typeof(bool));

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

        /// <summary>
        /// List of <see cref="StrokePathDTO" />s that make up the <see cref="StampedObject" />.
        /// </summary>
        public ObservableCollection<StrokePathDTO> StrokePaths
        {
            get { return GetValue<ObservableCollection<StrokePathDTO>>(StrokePathsProperty); }
            set { SetValue(StrokePathsProperty, value); }
        }

        public static readonly PropertyData StrokePathsProperty = RegisterProperty("StrokePaths", typeof(ObservableCollection<StrokePathDTO>), () => new ObservableCollection<StrokePathDTO>());

        #endregion //Binding
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class StampViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StampViewModel"/> class.
        /// </summary>
        public StampViewModel(Stamp stamp)
        {
            PageObject = stamp;
            if(App.MainWindowViewModel.ImagePool.ContainsKey(stamp.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[stamp.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(MainWindowViewModel.ImageCacheDirectory);
                foreach(var imageFilePath in from imageFilePath in imageFilePaths
                                             let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
                                             where imageHashID == stamp.ImageHashID
                                             select imageFilePath) 
                                             {
                                                 filePath = imageFilePath;
                                                 break;
                                             }

                var bitmapImage = CLPImage.GetImageFromPath(filePath);
                if(bitmapImage != null)
                {
                    SourceImage = bitmapImage;
                    App.MainWindowViewModel.ImagePool.Add(stamp.ImageHashID, bitmapImage);
                }
            }

            ParameterizeStampCommand = new Command(OnParameterizeStampCommandExecute);
            StartDragStampCommand = new Command(OnStartDragStampCommandExecute);
            PlaceStampCommand = new Command(OnPlaceStampCommandExecute);
            DragStampCommand = new Command<DragDeltaEventArgs>(OnDragStampCommandExecute);
            ShowKeyPadCommand = new Command(OnShowKeyPadCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StampVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Sets Stamp to CollectionStamp.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsCollectionStamp
        {
            get { return GetValue<bool>(IsCollectionStampProperty); }
            set { SetValue(IsCollectionStampProperty, value); }
        }

        public static readonly PropertyData IsCollectionStampProperty = RegisterProperty("IsCollectionStamp", typeof(bool));

        /// <summary>
        /// The number of parts the stamp represents.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// X offset for the ghost image of the <see cref="Stamp" /> as it's being dragged on the <see cref="CLPPage" />.
        /// </summary>
        public double GhostOffsetX
        {
            get { return GetValue<double>(GhostOffsetXProperty); }
            set { SetValue(GhostOffsetXProperty, value); }
        }

        public static readonly PropertyData GhostOffsetXProperty = RegisterProperty("GhostOffsetX", typeof(double), 0.0);

        /// <summary>
        /// Y offset for the ghost image of the <see cref="Stamp" /> as it's being dragged on the <see cref="CLPPage" />.
        /// </summary>
        public double GhostOffsetY
        {
            get { return GetValue<double>(GhostOffsetYProperty); }
            set { SetValue(GhostOffsetYProperty, value); }
        }

        public static readonly PropertyData GhostOffsetYProperty = RegisterProperty("GhostOffsetY", typeof(double), 0.0);

        /// <summary>
        /// Screenshot of the <see cref="Stamp" />'s body.
        /// </summary>
        public ImageSource GhostBodyImage
        {
            get { return GetValue<ImageSource>(GhostBodyImageProperty); }
            set { SetValue(GhostBodyImageProperty, value); }
        }

        public static readonly PropertyData GhostBodyImageProperty = RegisterProperty("GhostBodyImage", typeof(ImageSource));

        /// <summary>
        /// The visible image, loaded from the ImageCache.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof (ImageSource));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility PartsRegionVisibility
        {
            get { return GetValue<Visibility>(PartsRegionVisibilityProperty); }
            set { SetValue(PartsRegionVisibilityProperty, value); }
        }

        public static readonly PropertyData PartsRegionVisibilityProperty = RegisterProperty("PartsRegionVisibility", typeof(Visibility), Visibility.Visible);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public SolidColorBrush StampHandleColor
        {
            get { return GetValue<SolidColorBrush>(StampHandleColorProperty); }
            set { SetValue(StampHandleColorProperty, value); }
        }

        public static readonly PropertyData StampHandleColorProperty = RegisterProperty("StampHandleColor", typeof(SolidColorBrush), new SolidColorBrush(Colors.Black));

        #region Visibilities

        /// <summary>
        /// Visibility of the Ghost stamp.
        /// </summary>
        public bool IsGhostVisible
        {
            get { return GetValue<bool>(IsGhostVisibleProperty); }
            set
            {
                SetValue(IsGhostVisibleProperty, value);
                RaisePropertyChanged("IsDefaultAdornersVisible");
                IsAdornerVisible = IsGhostVisible;
            }
        }

        public static readonly PropertyData IsGhostVisibleProperty = RegisterProperty("IsGhostVisible", typeof(bool), false);

        /// <summary>
        /// Visibility of other adorners.
        /// </summary>
        public bool IsDefaultAdornersVisible
        {
            get { return !IsGhostVisible; }
        }

        #endregion //Visibilities

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Pops up keypad that allows parameterization of stamp copies.
        /// </summary>
        public Command ParameterizeStampCommand { get; private set; }

        private void OnParameterizeStampCommandExecute()
        {
            IsGhostVisible = false;
            var stamp = PageObject as Stamp;
            if (stamp == null)
            {
                return;
            }

            if (!HasParts() &&
                !IsCollectionStamp)
            {
                MessageBox.Show(
                                "What are you counting on the stamp?  Please click the questionmark on the line below the stamp before making copies.",
                                "What are you counting?");
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Pen;
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
                return;
            }

            var keyPad = new KeypadWindowView("How many stamp copies?", 21)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Top = 100,
                    Left = 100
                };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

            var serializedStrokes = new List<StrokeDTO>();
            foreach (var newStroke in stamp.AcceptedStrokes.Select(stroke => stroke.ToStrokeDTO().ToStroke()))
            {
                newStroke.SetStrokeID(Guid.NewGuid().ToCompactID());
                var transform = new Matrix();
                transform.Translate(-XPosition, -YPosition - stamp.HandleHeight);
                newStroke.Transform(transform, true);
                serializedStrokes.Add(newStroke.ToStrokeDTO());
            }

            var xPositionOffset = 0.0;
            var yPositionOffset = 0.0;
            var stampedObjectWidth = Width;
            var stampedObjectHeight = Height - stamp.HandleHeight - stamp.PartsHeight;
            if (!IsCollectionStamp &&
               stamp.ImageHashID == string.Empty) //Shrinks StampCopy to bounds of all strokePaths
            {
                var x1 = PageObject.ParentPage.Width;
                var y1 = PageObject.ParentPage.Height;
                var x2 = 0.0;
                var y2 = 0.0;
                foreach (var bounds in serializedStrokes.Select(serializedStroke => serializedStroke.ToStroke().GetBounds()))
                {
                    x1 = Math.Min(x1, bounds.Left);
                    y1 = Math.Min(y1, bounds.Top);
                    x2 = Math.Max(x2, bounds.Right);
                    y2 = Math.Max(y2, bounds.Bottom);
                }

                xPositionOffset = x1;
                yPositionOffset = y1;
                stampedObjectWidth = Math.Max(x2 - x1, 20); //TODO: center if too small?
                stampedObjectHeight = Math.Max(y2 - y1, 20);

                var transformedSerializedStrokes = new List<StrokeDTO>();
                foreach (var serializedStroke in serializedStrokes)
                {
                    var stroke = serializedStroke.ToStroke();
                    var transform = new Matrix();
                    transform.Translate(-x1, -y1);
                    stroke.Transform(transform, true);
                    transformedSerializedStrokes.Add(stroke.ToStrokeDTO());
                }
                serializedStrokes = transformedSerializedStrokes;
            }

            var initialXPosition = 25.0;
            var initialYPosition = YPosition + Height + 20;
            if (initialYPosition + stampedObjectHeight > PageObject.ParentPage.Height)
            {
                initialYPosition = PageObject.ParentPage.Height - stampedObjectHeight;
                initialXPosition = PageObject.XPosition + PageObject.Width + 10.0;
                if (initialXPosition + numberOfCopies * (stampedObjectWidth + 5) > PageObject.ParentPage.Width)
                {
                    initialXPosition = 25.0;
                }
            }
            var stampCopiesToAdd = new List<IPageObject>();
            for (var i = 0; i < numberOfCopies; i++)
            {
                var stampedObject = new StampedObject(stamp.ParentPage, stamp.ID, stamp.ImageHashID, IsCollectionStamp)
                {
                    Width = stampedObjectWidth,
                    Height = stampedObjectHeight,
                    XPosition = initialXPosition,
                    YPosition = initialYPosition,
                    SerializedStrokes = serializedStrokes,
                    Parts = stamp.Parts
                };

                stampCopiesToAdd.Add(stampedObject);
                if (initialXPosition + 2 * stampedObject.Width + 5 < PageObject.ParentPage.Width)
                {
                    initialXPosition += stampedObject.Width + 5;
                }
                else if (initialYPosition + 2 * stampedObject.Height + 5 < PageObject.ParentPage.Height)
                {
                    initialXPosition = 25;
                    initialYPosition += stampedObject.Height + 5;
                }

                foreach (var pageObject in stamp.AcceptedPageObjects)
                {
                    var newPageObject = pageObject.Duplicate();
                    newPageObject.XPosition = stampedObject.XPosition + (pageObject.XPosition - stamp.XPosition);
                    newPageObject.YPosition = stampedObject.YPosition + (pageObject.YPosition - stamp.YPosition - stamp.HandleHeight);
                    stampCopiesToAdd.Add(newPageObject);
                }
            }

            ACLPPageBaseViewModel.AddPageObjectsToPage(stamp.ParentPage, stampCopiesToAdd);
        }          

        /// <summary>
        /// Places copy of stamp below and displays StrokePathViews for dragging stamp.
        /// </summary>
        public Command StartDragStampCommand { get; private set; }

        bool _copyFailed;
        private void OnStartDragStampCommandExecute()
        {
            StampHandleColor = new SolidColorBrush(Colors.Black);
            _copyFailed = false;
            if (HasParts() || IsCollectionStamp)
            {
                GhostOffsetX = 0.0;
                GhostOffsetY = 0.0;
                
                //Take image of StampBody and add to Ghost border.
                var stamp = PageObject as Stamp;
                var pageViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(PageObject.ParentPage).First(x => (x is CLPPageViewModel) && !(x as CLPPageViewModel).IsPagePreview);
                var viewManager = Catel.IoC.ServiceLocator.Default.ResolveType<IViewManager>();
                var views = viewManager.GetViewsOfViewModel(pageViewModel);
                var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
                if(pageView == null ||
                   stamp == null)
                {
                    _copyFailed = true;
                    return;
                }

                const double SCREEN_DPI = 96.0;

                var renderTarget = new RenderTargetBitmap((int)PageObject.ParentPage.Width, (int)PageObject.ParentPage.Height, SCREEN_DPI, SCREEN_DPI, PixelFormats.Pbgra32);
                var sourceBrush = new VisualBrush(pageView);

                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();

                using(drawingContext)
                {
                    drawingContext.PushTransform(new ScaleTransform(1.0, 1.0));
                    drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(PageObject.ParentPage.Width, PageObject.ParentPage.Height)));
                }
                renderTarget.Render(drawingVisual);

                var crop = new CroppedBitmap(renderTarget, new Int32Rect((int)(XPosition + 2), (int)(YPosition + stamp.HandleHeight + 2), (int)(Width - 4), (int)(Height - stamp.HandleHeight - stamp.PartsHeight - 4)));
                
                byte[] imageArray;
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(crop));
                using(var outputStream = new MemoryStream())
                {
                    encoder.Save(outputStream);
                    imageArray = outputStream.ToArray();
                }

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                bitmapImage.StreamSource = new MemoryStream(imageArray);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                GhostBodyImage = bitmapImage;
                IsGhostVisible = true;
            } 
            else 
            {
                MessageBox.Show("What are you counting on the stamp?  Please click the questionmark on the line below the stamp before making copies.", "What are you counting?");
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Pen;
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
                _copyFailed = true;
            }  
        }

        /// <summary>
        /// Stamp Dragged By Handle
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            if(_copyFailed)
            {
                IsGhostVisible = false;
                return;
            }

            var parentPage = PageObject.ParentPage;
            var stamp = PageObject as Stamp;
            var newOffsetX = Math.Max(-XPosition, e.HorizontalChange);
            newOffsetX = Math.Min(newOffsetX, parentPage.Width - Width - XPosition);
            var newOffsetY = Math.Max(-YPosition - stamp.HandleHeight, e.VerticalChange);
            newOffsetY = Math.Min(newOffsetY, parentPage.Height - Height - YPosition + stamp.PartsHeight);

            GhostOffsetX = newOffsetX;
            GhostOffsetY = newOffsetY;
        }

        /// <summary>
        /// Copies StampCopy to page on Stamp Placed (DragCompleted Event)
        /// </summary>
        public Command PlaceStampCommand { get; private set; }

        private void OnPlaceStampCommandExecute()
        {
            IsGhostVisible = false;
            var stamp = PageObject as Stamp;
            if(_copyFailed ||
               stamp == null)
            {
                return;
            }

            var deltaX = Math.Abs(GhostOffsetX);
            var deltaY = Math.Abs(GhostOffsetY);

            if(deltaX < Width + 5 &&
               deltaY < Height - stamp.PartsHeight)
            {
                return;
            }
            
            var stampedObject = new StampedObject(stamp.ParentPage, stamp.ID, stamp.ImageHashID, IsCollectionStamp)
                                {
                                    Width = Width,
                                    Height = Height - stamp.HandleHeight - stamp.PartsHeight,
                                    XPosition = stamp.XPosition + GhostOffsetX,
                                    YPosition = stamp.YPosition + GhostOffsetY + stamp.HandleHeight,
                                    Parts = stamp.Parts
                                };

            foreach (var stroke in stamp.AcceptedStrokes)
            {
                var newStroke = stroke.ToStrokeDTO().ToStroke();
                newStroke.SetStrokeID(Guid.NewGuid().ToCompactID());
                var transform = new Matrix();
                transform.Translate(-XPosition, -YPosition - stamp.HandleHeight);
                newStroke.Transform(transform, true);
                stampedObject.SerializedStrokes.Add(newStroke.ToStrokeDTO());
            }

            var xPosition = stampedObject.XPosition;
            var yPosition = stampedObject.YPosition;
            if(!IsCollectionStamp && 
               stampedObject.ImageHashID == string.Empty) //Shrinks StampCopy to bounds of all strokePaths
            {
                var x1 = Double.MaxValue;
                var y1 = Double.MaxValue;
                var x2 = 0.0;
                var y2 = 0.0;
                foreach(var bounds in stampedObject.SerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke().GetBounds()))
                {
                    x1 = Math.Min(x1, bounds.Left);
                    y1 = Math.Min(y1, bounds.Top);
                    x2 = Math.Max(x2, bounds.Right);
                    y2 = Math.Max(y2, bounds.Bottom);
                }

                xPosition += x1;
                yPosition += y1;
                stampedObject.Width = Math.Max(x2 - x1, 20); //TODO: center if too small?
                stampedObject.Height = Math.Max(y2 - y1, 20);

                var transformedSerializedStrokes = new List<StrokeDTO>();
                foreach(var serializedStroke in stampedObject.SerializedStrokes)
                {
                    var stroke = serializedStroke.ToStroke();
                    var transform = new Matrix();
                    transform.Translate(-x1, -y1);
                    stroke.Transform(transform, true);
                    transformedSerializedStrokes.Add(stroke.ToStrokeDTO());
                }
                stampedObject.SerializedStrokes = transformedSerializedStrokes;
            }

            stampedObject.XPosition = xPosition;
            stampedObject.YPosition = yPosition;

            var combinedPageObjects = new List<IPageObject>
                                      {
                                          stampedObject
                                      };
            foreach(var pageObject in stamp.AcceptedPageObjects)
            {
                var newPageObject = pageObject.Duplicate();
                newPageObject.XPosition = pageObject.XPosition + GhostOffsetX;
                newPageObject.YPosition = pageObject.YPosition + GhostOffsetY;
                combinedPageObjects.Add(newPageObject);
            }

            ACLPPageBaseViewModel.AddPageObjectsToPage(stampedObject.ParentPage, combinedPageObjects);
        }

        /// <summary>
        /// Shows Modal Window Keypad to input Parts manually.
        /// </summary>
        public Command ShowKeyPadCommand { get; private set; }
        
        private void OnShowKeyPadCommandExecute()
        {
            var stamp = PageObject as Stamp;
            if(stamp == null ||
               (!App.MainWindowViewModel.IsAuthoring && stamp.IsPartsAutoGenerated))
            {
                return;
            }

            var keyPad = new KeypadWindowView("How many things are you\ncounting on the stamp?", 100)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual,
                             Top = 100,
                             Left = 100
                         };
            keyPad.ShowDialog();
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var oldParts = stamp.Parts;
            var parts = Int32.Parse(keyPad.NumbersEntered.Text);
            stamp.Parts = parts;
        //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPartsChanged(PageObject.ParentPage, PageObject.UniqueID, oldParts));
            if(App.MainWindowViewModel.IsAuthoring)
            {
                stamp.IsPartsAutoGenerated = true;
            }
        }

        #endregion //Commands
        
        #region Methods

        private bool HasParts()
        {
            var stamp = PageObject as Stamp;
            if (stamp == null)
            {
                return false;
            }

            if(stamp.IsPartsAutoGenerated)
            {
                return true;
            }

            return stamp.Parts > 0;
        }

        #endregion //Methods
    }
}

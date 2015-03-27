using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;
using RibbonButton = CLP.CustomControls.RibbonButton;

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
        public StampViewModel(Stamp stamp, INotebookService notebookService)
        {
            PageObject = stamp;
            stamp.AcceptedStrokes = stamp.AcceptedStrokeParentIDs.Select(id => PageObject.ParentPage.GetStrokeByID(id)).ToList();
            RaisePropertyChanged("IsGroupStamp");
            RaisePropertyChanged("IsDraggableStamp");
            if(App.MainWindowViewModel.ImagePool.ContainsKey(stamp.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[stamp.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(notebookService.CurrentImageCacheDirectory);
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
            ToggleChildBoundaryVisibilityCommand = new Command(OnToggleChildBoundaryVisibilityCommandExecute);
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Images/AddToDisplay.png", ParameterizeStampCommand, null, true));

            var toggleChildPartsButton = new ToggleRibbonButton("Show Group Sizes", "Hide Group Sizes", "pack://application:,,,/Resources/Images/WindowControls/RestoreButton.png", true)
            {
                IsChecked = IsPartsLabelVisibleForChildren
            };
            toggleChildPartsButton.Checked += toggleChildPartsButton_Checked;
            toggleChildPartsButton.Unchecked += toggleChildPartsButton_Checked;
            _contextButtons.Add(toggleChildPartsButton);

            if (IsGroupStamp)
            {
                return;
            }

            var toggleChildBoundariesButton = new ToggleRibbonButton("Show Borders", "Hide Borders", "pack://application:,,,/Resources/Images/WindowControls/RestoreButton.png", true)
                                              {
                                                  IsChecked = IsBoundaryVisibleForChildren
                                              };
            toggleChildBoundariesButton.Checked += toggleChildBoundariesButton_Checked;
            toggleChildBoundariesButton.Unchecked += toggleChildBoundariesButton_Checked;
            _contextButtons.Add(toggleChildBoundariesButton);
        }

        void toggleChildPartsButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null ||
                button.IsChecked == null)
            {
                return;
            }

            IsPartsLabelVisibleForChildren = (bool)button.IsChecked;

            foreach (var stampedObject in PageObject.ParentPage.PageObjects.OfType<StampedObject>().Where(x => x.ParentStampID == PageObject.ID))
            {
                stampedObject.IsPartsLabelVisible = IsPartsLabelVisibleForChildren && !IsGroupStamp;
            }
        }

        void toggleChildBoundariesButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null ||
                button.IsChecked == null)
            {
                return;
            }

            IsBoundaryVisibleForChildren = (bool)button.IsChecked;

            foreach (var stampedObject in PageObject.ParentPage.PageObjects.OfType<StampedObject>().Where(x => x.ParentStampID == PageObject.ID))
            {
                stampedObject.IsBoundaryVisible = !IsGroupStamp && IsBoundaryVisibleForChildren;
            }
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StampVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The type of <see cref="Stamp" />.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public StampTypes StampType
        {
            get { return GetValue<StampTypes>(StampTypeProperty); }
            set { SetValue(StampTypeProperty, value); }
        }

        public static readonly PropertyData StampTypeProperty = RegisterProperty("StampType", typeof(StampTypes));

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
        /// Is Stamp a CollectionStamp.
        /// </summary>
        public bool IsGroupStamp
        {
            get
            {
                var stamp = PageObject as Stamp;
                if (stamp == null)
                {
                    return false;
                }

                return StampType == StampTypes.GroupStamp || StampType == StampTypes.EmptyGroupStamp;
            }
        }

        public bool IsDraggableStamp
        {
            get
            {
                var stamp = PageObject as Stamp;
                if (stamp == null)
                {
                    return false;
                }

                return StampType == StampTypes.GroupStamp || StampType == StampTypes.GeneralStamp;
            }
        }

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

        #region Properties

        /// <summary>
        /// Toggles the visibility of a boundary around the stampedObject.
        /// </summary>
        public bool IsBoundaryVisibleForChildren
        {
            get { return GetValue<bool>(IsBoundaryVisibleForChildrenProperty); }
            set { SetValue(IsBoundaryVisibleForChildrenProperty, value); }
        }

        public static readonly PropertyData IsBoundaryVisibleForChildrenProperty = RegisterProperty("IsBoundaryVisibleForChildren", typeof(bool), true);

        /// <summary>
        /// Toggles the visibility of Parts info for the Stamp's stampedObject children.
        /// </summary>
        public bool IsPartsLabelVisibleForChildren
        {
            get { return GetValue<bool>(IsPartsLabelVisibleForChildrenProperty); }
            set { SetValue(IsPartsLabelVisibleForChildrenProperty, value); }
        }

        public static readonly PropertyData IsPartsLabelVisibleForChildrenProperty = RegisterProperty("IsPartsLabelVisibleForChildren", typeof (bool), false);

        #endregion //Properties

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
                !IsGroupStamp)
            {
                MessageBox.Show("How many are in the group?", "What are you counting?");
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
                return;
            }

            var keypadPrompt = IsDraggableStamp
                ? StampType == StampTypes.GeneralStamp ? "How many copies?" : "How many groups?"
                : StampType == StampTypes.ObservingStamp ? "How many objects?" : "How many groups?";

            var keypadLimit = StampType == StampTypes.EmptyGroupStamp || StampType == StampTypes.GroupStamp ? 21 : 101;

            var keyPad = new KeypadWindowView(keypadPrompt, keypadLimit)
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

            var stampedObjectWidth = Width;
            var stampedObjectHeight = Height - stamp.HandleHeight - stamp.PartsHeight;
            if (!IsGroupStamp &&
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

            var stampObjectType = stamp.StampType == StampTypes.GeneralStamp || stamp.StampType == StampTypes.ObservingStamp
                                      ? StampedObjectTypes.GeneralStampedObject
                                      : stamp.StampType == StampTypes.GroupStamp
                                            ? StampedObjectTypes.GroupStampedObject
                                            : StampedObjectTypes.EmptyGroupStampedObject;

            var stampCopiesToAdd = new List<IPageObject>();

            if (stamp.StampType == StampTypes.ObservingStamp)
            {
                var random = new Random();
                var miniGroupingXPosition = 5.0;
                var miniGroupingYPosition = YPosition + Height + 100;
                var miniGroupingColumns = stampedObjectWidth / stampedObjectHeight <= 1.0 ? 3 : 2;
                var miniGroupingRows = miniGroupingColumns == 3 ? 2 : 3;
                var miniGroupingWidth = stampedObjectWidth * miniGroupingColumns + 10;
                var miniGroupingHeight = stampedObjectHeight * miniGroupingRows + 10;
                for (var i = 0; i < numberOfCopies; i++)
                {
                    var miniGroupIndex = i % 6;
                    var xOffset = (miniGroupIndex % miniGroupingColumns) * stampedObjectWidth + (miniGroupIndex % miniGroupingColumns == 0 ? 0 : 5) -
                                  8 + random.NextDouble() * 16;
                    var yOffset = (miniGroupIndex % miniGroupingRows) * stampedObjectHeight + (miniGroupIndex % miniGroupingRows == 0 ? 0 : 5) - 8 +
                                  random.NextDouble() * 16;

                    var stampedObject = new StampedObject(stamp.ParentPage, stamp.ID, stamp.ImageHashID, stampObjectType)
                                        {
                                            Width = stampedObjectWidth,
                                            Height = stampedObjectHeight,
                                            XPosition = miniGroupingXPosition + xOffset,
                                            YPosition = miniGroupingYPosition + yOffset,
                                            SerializedStrokes = serializedStrokes.Select(stroke => stroke.ToStroke().ToStrokeDTO()).ToList(),
                                            Parts = stamp.Parts,
                                            IsBoundaryVisible = !IsGroupStamp && IsBoundaryVisibleForChildren,
                                        };

                    stampCopiesToAdd.Add(stampedObject);
                    if (miniGroupIndex == 5)
                    {
                        if (miniGroupingXPosition + 2 * miniGroupingWidth < PageObject.ParentPage.Width)
                        {
                            miniGroupingXPosition += miniGroupingWidth;
                        }
                        else if (miniGroupingYPosition + 2 * miniGroupingHeight < PageObject.ParentPage.Height)
                        {
                            miniGroupingXPosition = 5.0;
                            miniGroupingYPosition += miniGroupingHeight;
                        }
                    }

                    foreach (var pageObject in stamp.AcceptedPageObjects)
                    {
                        var newPageObject = pageObject.Duplicate();
                        newPageObject.XPosition = stampedObject.XPosition + (pageObject.XPosition - stamp.XPosition);
                        newPageObject.YPosition = stampedObject.YPosition + (pageObject.YPosition - stamp.YPosition - stamp.HandleHeight);
                        stampCopiesToAdd.Add(newPageObject);
                    }
                }
            }
            else
            {
                var initialXPosition = 25.0;
                var initialYPosition = YPosition + Height + 125;
                if (initialYPosition + stampedObjectHeight > PageObject.ParentPage.Height)
                {
                    initialYPosition = PageObject.ParentPage.Height - stampedObjectHeight;
                    initialXPosition = PageObject.XPosition + PageObject.Width + 10.0;
                    if (initialXPosition + numberOfCopies * (stampedObjectWidth + 5) > PageObject.ParentPage.Width)
                    {
                        initialXPosition = 25.0;
                    }
                }

                for (var i = 0; i < numberOfCopies; i++)
                {
                    var stampedObject = new StampedObject(stamp.ParentPage, stamp.ID, stamp.ImageHashID, stampObjectType)
                    {
                        Width = stampedObjectWidth,
                        Height = stampedObjectHeight,
                        XPosition = initialXPosition,
                        YPosition = initialYPosition,
                        SerializedStrokes = serializedStrokes.Select(stroke => stroke.ToStroke().ToStrokeDTO()).ToList(),
                        Parts = stamp.Parts,
                        IsBoundaryVisible = !IsGroupStamp && IsBoundaryVisibleForChildren,
                        IsPartsLabelVisible = IsPartsLabelVisibleForChildren && !IsGroupStamp
                    };

                    stampCopiesToAdd.Add(stampedObject);
                    if (initialXPosition + 2 * stampedObject.Width + 15 < PageObject.ParentPage.Width)
                    {
                        initialXPosition += stampedObject.Width + 15;
                    }
                    else if (initialYPosition + 2 * stampedObject.Height + 15 < PageObject.ParentPage.Height)
                    {
                        initialXPosition = 25;
                        initialYPosition += stampedObject.Height + 15;
                    }

                    foreach (var pageObject in stamp.AcceptedPageObjects)
                    {
                        var newPageObject = pageObject.Duplicate();
                        newPageObject.XPosition = stampedObject.XPosition + (pageObject.XPosition - stamp.XPosition);
                        newPageObject.YPosition = stampedObject.YPosition + (pageObject.YPosition - stamp.YPosition - stamp.HandleHeight);
                        stampCopiesToAdd.Add(newPageObject);
                    }
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
            if (!IsDraggableStamp)
            {
                return;
            }

            if (!HasParts() && !IsGroupStamp)
            {
                _copyFailed = true;
                IsGhostVisible = false;
                StampHandleColor = new SolidColorBrush(Colors.Red);
                return;
            }

            StampHandleColor = new SolidColorBrush(Colors.Black);
            _copyFailed = false;
            GhostOffsetX = 0.0;
            GhostOffsetY = 0.0;

            //Take image of StampBody and add to Ghost border.
            var stamp = PageObject as Stamp;
            var pageViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(PageObject.ParentPage).First(x => (x is CLPPageViewModel) && !(x as CLPPageViewModel).IsPagePreview);
            var viewManager = Catel.IoC.ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null ||
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

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(1.0, 1.0));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(PageObject.ParentPage.Width, PageObject.ParentPage.Height)));
            }
            renderTarget.Render(drawingVisual);

            var crop = new CroppedBitmap(renderTarget, new Int32Rect((int)(XPosition + 2), (int)(YPosition + stamp.HandleHeight + 2), (int)(Width - 4), (int)(Height - stamp.HandleHeight - stamp.PartsHeight - 4)));

            byte[] imageArray;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(crop));
            using (var outputStream = new MemoryStream())
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
            PopulateContextRibbon(true);
        }

        /// <summary>
        /// Stamp Dragged By Handle
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            if (!IsDraggableStamp)
            {
                IsGhostVisible = false;
                return;
            }

            if (!HasParts() && !IsGroupStamp)
            {
                _copyFailed = true;
                IsGhostVisible = false;
                StampHandleColor = new SolidColorBrush(Colors.Red);
                return;
            }

            if(_copyFailed)
            {
                IsGhostVisible = false;
                return;
            }

            var parentPage = PageObject.ParentPage;
            var stamp = PageObject as Stamp;
            if (stamp == null)
            {
                return;
            }
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
            if (!IsDraggableStamp)
            {
                IsGhostVisible = false;
                return;
            }

            if (!HasParts() && !IsGroupStamp)
            {
                _copyFailed = true;
                IsGhostVisible = false;
                StampHandleColor = new SolidColorBrush(Colors.Black);
                MessageBox.Show("How many are in the group?", "What are you counting?");
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
                return;
            }

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

            var stampObjectType = stamp.StampType == StampTypes.GeneralStamp || stamp.StampType == StampTypes.ObservingStamp
                                      ? StampedObjectTypes.GeneralStampedObject
                                      : stamp.StampType == StampTypes.GroupStamp
                                            ? StampedObjectTypes.GroupStampedObject
                                            : StampedObjectTypes.EmptyGroupStampedObject;

            var stampedObject = new StampedObject(stamp.ParentPage, stamp.ID, stamp.ImageHashID, stampObjectType)
                                {
                                    Width = Width,
                                    Height = Height - stamp.HandleHeight - stamp.PartsHeight,
                                    XPosition = stamp.XPosition + GhostOffsetX,
                                    YPosition = stamp.YPosition + GhostOffsetY + stamp.HandleHeight,
                                    Parts = stamp.Parts,
                                    IsBoundaryVisible = !IsGroupStamp && IsBoundaryVisibleForChildren,
                                    IsPartsLabelVisible = IsPartsLabelVisibleForChildren && !IsGroupStamp
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
            if(!IsGroupStamp && 
               stampedObject.ImageHashID == string.Empty) //Shrinks StampCopy to bounds of all strokePaths
            {
                var x1 = PageObject.ParentPage.Width;
                var y1 = PageObject.ParentPage.Height;
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

            var keyPad = new KeypadWindowView("How many are in the group?", 101)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
                         };
            keyPad.ShowDialog();
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var oldPartsValue = stamp.Parts;
            var parts = Int32.Parse(keyPad.NumbersEntered.Text);
            stamp.Parts = parts;
            ACLPPageBaseViewModel.AddHistoryItemToPage(stamp.ParentPage,
                                                       new PartsValueChangedHistoryItem(stamp.ParentPage, App.MainWindowViewModel.CurrentUser, stamp.ID, oldPartsValue));
            if(App.MainWindowViewModel.IsAuthoring)
            {
                stamp.IsPartsAutoGenerated = true;
            }
        }

        /// <summary>
        /// Toggles the boundary visibilities of all children stamps.
        /// </summary>
        public Command ToggleChildBoundaryVisibilityCommand { get; private set; }

        private void OnToggleChildBoundaryVisibilityCommandExecute()
        {
            IsBoundaryVisibleForChildren = !IsBoundaryVisibleForChildren;

            foreach (var stampedObject in PageObject.ParentPage.PageObjects.OfType<StampedObject>().Where(x => x.ParentStampID == PageObject.ID))
            {
                stampedObject.IsBoundaryVisible = IsBoundaryVisibleForChildren;
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

        protected override void PopulateContextRibbon(bool isChangedValueMeaningful)
        {
            if (isChangedValueMeaningful)
            {
                ContextRibbon.Buttons.Clear();
            }

            if (!IsDefaultAdornersVisible || 
                !IsAdornerVisible)
            {
                return;
            }

            ContextRibbon.Buttons = new ObservableCollection<UIElement>(_contextButtons);
        }

        #endregion //Methods

        #region Static Methods

        public static void AddBlankGeneralStampToPage(CLPPage page)
        {
            var stamp = new Stamp(page, StampTypes.GeneralStamp);
            ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
        }

        public static void AddBlankGroupStampToPage(CLPPage page)
        {
            var stamp = new Stamp(page, StampTypes.GroupStamp);
            ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
        }

        public static void AddImageGeneralStampToPage(CLPPage page)
        {
            CreateImageStamp(StampTypes.GeneralStamp, page);
        }

        public static void AddImageGroupStampToPage(CLPPage page)
        {
            CreateImageStamp(StampTypes.GroupStamp, page);
        }

        private static void CreateImageStamp(StampTypes stampType, CLPPage page)
        {
            // Configure open file dialog box
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"
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

                var stamp = new Stamp(page, imageHashID, stampType);

                ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
            }
            else
            {
                MessageBox.Show("Error opening image file. Please try again.");
            }
        }

        public static void AddPileToPage(CLPPage page)
        {
            var pageObjectsToAdd = new List<IPageObject>();
            var observerStamp = new Stamp(page, StampTypes.ObservingStamp);
            pageObjectsToAdd.Add(observerStamp);
            var emptyGroupStamp = new Stamp(page, StampTypes.EmptyGroupStamp);
            pageObjectsToAdd.Add(emptyGroupStamp);
            ACLPPageBaseViewModel.AddPageObjectsToPage(page, pageObjectsToAdd);
        }

        #endregion //Static Methods
    }
}

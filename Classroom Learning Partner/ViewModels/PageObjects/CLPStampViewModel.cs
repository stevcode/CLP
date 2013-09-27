using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows.Controls;
using CLP.Models;
using Classroom_Learning_Partner.Views.Modal_Windows;


namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStampViewModel : ACLPPageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampViewModel"/> class.
        /// </summary>
        public CLPStampViewModel(CLPStamp stamp)
        {
            PageObject = stamp;

            ResizeStampCommand = new Command<DragDeltaEventArgs>(OnResizeStampCommandExecute);
            CopyStampCommand = new Command(OnCopyStampCommandExecute);
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
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public CLPStampCopy StampCopy
        {
            get { return GetValue<CLPStampCopy>(StampCopyProperty); }
            set { SetValue(StampCopyProperty, value); }
        }

        public static readonly PropertyData StampCopyProperty = RegisterProperty("StampCopy", typeof(CLPStampCopy));

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

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Resize a stamp.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeStampCommand { get; private set; }

        private void OnResizeStampCommandExecute(DragDeltaEventArgs e)
        {
            var stamp = PageObject as CLPStamp;
            if(stamp == null)
            {
                return;
            }
            var oldHeight = Height;
            var oldWidth = Width;

            const double MIN_WIDTH = 50.0;
            const double MIN_HEIGHT = 140.0;
            var newHeight = Height + e.VerticalChange;
            var newWidth = Width + e.HorizontalChange;
            if(newHeight < MIN_HEIGHT)
            {
                newHeight = MIN_HEIGHT;
            }
            if(newWidth < MIN_WIDTH)
            {
                newWidth = MIN_WIDTH;
            }
            if(newHeight + YPosition > PageObject.ParentPage.PageHeight)
            {
                newHeight = Height;
            }
            if(newWidth + XPosition > PageObject.ParentPage.PageWidth)
            {
                newWidth = Width;
            }

            Height = newHeight;
            StampCopy.Height = newHeight - CLPStamp.HandleHeight - CLPStamp.PartsHeight;
            Width = newWidth;
            StampCopy.Width = newWidth;

            //Resize History
            var heightDiff = Math.Abs(oldHeight - Height);
            var widthDiff = Math.Abs(oldWidth - Width);
            var diff = heightDiff + widthDiff;
            if(!(diff > CLPHistory.SAMPLE_RATE))
            {
                return;
            }

            var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            if(batch is CLPHistoryPageObjectResizeBatch)
            {
                (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID,
                                                                                 new Point(Width, Height));
            }
            else
            {
                PageObject.ParentPage.PageHistory.EndBatch();
                //TODO: log this error
            }
        }      

        /// <summary>
        /// Places copy of stamp below and displays StrokePathViews for dragging stamp.
        /// </summary>
        public Command CopyStampCommand { get; private set; }

        private void OnCopyStampCommandExecute()
        { 
            StampHandleColor = new SolidColorBrush(Colors.Green);
            if (HasParts())
            {
                PartsRegionVisibility = Visibility.Collapsed;
                CopyStamp(PageObject.ParentPage.PageObjects.IndexOf(PageObject));

                var originalStrokes = PageObject.GetStrokesOverPageObject();
                var clonedStrokes = new StrokeCollection();

                foreach (var stroke in originalStrokes)
                {
                    var newStroke = (new StrokeDTO(stroke)).ToStroke();
                    var transform = new Matrix();
                    transform.Translate(-XPosition, -YPosition - CLPStamp.HandleHeight);
                    newStroke.Transform(transform, true);
                    clonedStrokes.Add(newStroke);
                }

                StampCopy.SerializedStrokes = StrokeDTO.SaveInkStrokes(clonedStrokes);
                StampCopy.IsStamped = true;

                foreach(var pageObject in PageObject.GetPageObjectsOverPageObject())
                {
                    var pageObjectViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(pageObject as ModelBase).FirstOrDefault();
                    if(pageObjectViewModel == null)
                    {
                        continue;
                    }
                    var pageObjectView = CLPServiceAgent.Instance.GetViewFromViewModel(pageObjectViewModel);
                    var imageByteSource = CLPServiceAgent.Instance.GetJpgImage(pageObjectView as UIElement);
                    var image = CLPImageViewModel.LoadImageFromByteSource(imageByteSource);
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    String photolocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\blah.jpg";  //file name 

                    encoder.Frames.Add(BitmapFrame.Create(image));

                    using(var filestream = new FileStream(photolocation, FileMode.Create))
                        encoder.Save(filestream);

                    var collectedImage = new CLPCollectedPartImage
                                         {
                                             Height = pageObject.Height,
                                             Width = pageObject.Width,
                                             XPosition = pageObject.XPosition - XPosition,
                                             YPosition = pageObject.YPosition - YPosition - CLPStamp.HandleHeight
                                         };
                    StampCopy.CollectedPartImages.Add(collectedImage);
                }
            } 
            else 
            {
                MessageBox.Show("What are you counting on the stamp?  Please write the number on the line below the stamp before making copies.", "What are you counting?");
            }  
        }

        double _originalX;
        double _originalY;

        private void CopyStamp(int stampIndex)
        {
            IsAdornerVisible = false;
            IsMouseOverShowEnabled = false;

            try
            {
                var leftBehindStamp = PageObject.Duplicate() as CLPStamp;
                var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
                if(notebookWorkspaceViewModel == null || leftBehindStamp == null)
                {
                    return;
                }

                leftBehindStamp.UniqueID = PageObject.UniqueID;

                _originalX = leftBehindStamp.XPosition;
                _originalY = leftBehindStamp.YPosition;

                if(stampIndex > -1)
                {
                    ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, leftBehindStamp, false, false, stampIndex);
                }
                else
                {
                    ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, leftBehindStamp, false);
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("[ERROR]: Failed to copy left behind container. " + ex.Message);
            }
        }

        /// <summary>
        /// Copies StampCopy to page on Stamp Placed (DragCompleted Event)
        /// </summary>
        public Command PlaceStampCommand { get; private set; }

        private void OnPlaceStampCommandExecute()
        {
            if(!HasParts())
            {
                return;
            }

            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
            var deltaX = Math.Abs(PageObject.XPosition - _originalX);
            var deltaY = Math.Abs(PageObject.YPosition - _originalY);

            if(deltaX < PageObject.Width + 5 &&
               deltaY < PageObject.Height)
            {
                return;
            }


            var xPosition = PageObject.XPosition;
            var yPosition = PageObject.YPosition + CLPStamp.HandleHeight;
            if(!IsCollectionStamp && StampCopy.ImageID == string.Empty) //Shrinks StampCopy to bounds of all strokePaths
            {
                double x1 = Double.MaxValue, y1 = Double.MaxValue, x2 = 0, y2 = 0;
                var copyStrokes = StrokeDTO.LoadInkStrokes(StampCopy.SerializedStrokes);
                foreach(var bounds in copyStrokes.Select(stroke => stroke.GetBounds()))
                {
                    x1 = Math.Min(x1, bounds.Left);
                    y1 = Math.Min(y1, bounds.Top);
                    x2 = Math.Max(x2, bounds.Right);
                    y2 = Math.Max(y2, bounds.Bottom);
                }

                xPosition += x1; //if positive?
                yPosition += y1; //if positive?
                StampCopy.Width = Math.Max(x2 - x1, 20); //TODO: center if too small?
                StampCopy.Height = Math.Max(y2 - y1, 20);

                foreach(var stroke in copyStrokes)
                {
                    var transform = new Matrix();
                    transform.Translate(-x1, -y1);
                    stroke.Transform(transform, true);
                }
                StampCopy.SerializedStrokes = StrokeDTO.SaveInkStrokes(copyStrokes);
            }

            StampCopy.XPosition = xPosition;
            StampCopy.YPosition = yPosition;
            StampCopy.ParentID = PageObject.UniqueID;
            StampCopy.UniqueID = Guid.NewGuid().ToString();
            StampCopy.Parts = PageObject.Parts;
            StampCopy.IsInternalPageObject = false;
            StampCopy.IsCollectionCopy = IsCollectionStamp;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                var parentPage = notebookWorkspaceViewModel.Notebook.GetNotebookPageByID(PageObject.ParentPageID);
                ACLPPageBaseViewModel.AddPageObjectToPage(parentPage, StampCopy, false, false);
            }

            PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryStampPlace(PageObject.ParentPage, StampCopy.UniqueID));
        }

        /// <summary>
        /// Stamp Dragged By Adorner
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            IsAdornerVisible = false;
            IsMouseOverShowEnabled = false;

            var x = PageObject.XPosition + e.HorizontalChange;
            var y = PageObject.YPosition + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < - CLPStamp.HandleHeight)
            {
                y = -CLPStamp.HandleHeight;
            }
            if (x > PageObject.ParentPage.PageWidth - PageObject.Width)
            {
                x = PageObject.ParentPage.PageWidth - PageObject.Width;
            }
            if(y > PageObject.ParentPage.PageHeight - PageObject.Height + CLPStamp.PartsHeight)
            {
                y = PageObject.ParentPage.PageHeight - PageObject.Height + CLPStamp.PartsHeight;
            }

            var xDelta = x - PageObject.XPosition;
            var yDelta = y - PageObject.YPosition;

            //TODO: Remove this in favor of jpg's of views?
            //foreach (var pageObject in PageObject.GetPageObjectsOverPageObject()) 
            //{
            //    var pageObjectPt = new Point((xDelta + pageObject.XPosition), (yDelta + pageObject.YPosition));
            //    ChangePageObjectPosition(pageObject, pageObjectPt);
            //}

            var pt = new Point(x, y);

            ChangePageObjectPosition(PageObject, pt);
        }

        /// <summary>
        /// Shows Modal Window Keypad to input Parts manually.
        /// </summary>
        public Command ShowKeyPadCommand { get; private set; }
        
        private void OnShowKeyPadCommandExecute()
        {
            var clpStamp = PageObject as CLPStamp;
            if(clpStamp != null && (App.MainWindowViewModel.IsAuthoring || !clpStamp.PartsAuthorGenerated))
            {
                var keyPad = new KeypadWindowView
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Top = 100,
                        Left = 100
                    };
                keyPad.ShowDialog();
                if(keyPad.DialogResult == true && keyPad.NumbersEntered.Text.Length > 0)
                {
                    PageObject.Parts = Int32.Parse(keyPad.NumbersEntered.Text);
                    if(App.MainWindowViewModel.IsAuthoring)
                    {
                        (PageObject as CLPStamp).PartsAuthorGenerated = true;
                    }
                }
            }
        }

        #endregion //Commands
        
        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(hitBoxName == "StampHandleHitBox")
            {
                if(IsBackground)
                {
                    if(App.MainWindowViewModel.IsAuthoring)
                    {
                        OpenAdornerTimeOut = 0.0;
                        IsMouseOverShowEnabled = true;
                    }
                    else
                    {
                        IsMouseOverShowEnabled = false;
                    }
                }
                else
                {
                    OpenAdornerTimeOut = 0.8;
                    IsMouseOverShowEnabled = true;
                }
                return false;
            }
            if(hitBoxName == "StampBodyHitBox")
            {
                return true;
            }
            if (hitBoxName == "PartsHitBox")
            {
                IsMouseOverShowEnabled = IsAdornerVisible;
            }

            return false;
        }

        public override void EraserHitTest(string hitBoxName, object tag)
        {
            if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                return;
            }
            
            if(hitBoxName == "StampHandleHitBox")
            {
                var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
                if(notebookWorkspaceViewModel != null)
                {
                    var parentPage = notebookWorkspaceViewModel.Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                    if(parentPage != null)
                    {
                        foreach(CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(parentPage))
                        {
                            pageVM.IsInkCanvasHitTestVisible = true;
                        }

                        ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
                    }
                }
            }
        }

        private bool HasParts()
        {
            var clpStamp = PageObject as CLPStamp;
            if (clpStamp != null && clpStamp.PartsAuthorGenerated)
            {
                return true;
            }
            return PageObject.Parts > 0;
        }

        #endregion //Methods
    }
}

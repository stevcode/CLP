using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
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

            ParameterizeStampCommand = new Command(OnParameterizeStampCommandExecute);
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
        /// Pops up keypad that allows parameterization of stamp copies.
        /// </summary>
        public Command ParameterizeStampCommand { get; private set; }

        private void OnParameterizeStampCommandExecute()
        {
            if(HasParts() || IsCollectionStamp)
            {
                var keyPad = new KeypadWindowView("How many stamp copies?", 21)
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

                var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);
                var originalStrokes = PageObject.GetStrokesOverPageObject();
                var clonedStrokes = new StrokeCollection();

                foreach(var stroke in originalStrokes)
                {
                    var newStroke = (new StrokeDTO(stroke)).ToStroke();
                    var transform = new Matrix();
                    transform.Translate(-XPosition, -YPosition - CLPStamp.HandleHeight);
                    newStroke.Transform(transform, true);
                    clonedStrokes.Add(newStroke);
                }

                StampCopy.SerializedStrokes = StrokeDTO.SaveInkStrokes(clonedStrokes);
                    
                //TODO: clipping

                var initialXPosition = 25.0;
                var initialYPosition = YPosition + Height + 20;
                if(initialYPosition + StampCopy.Height > PageObject.ParentPage.PageHeight)
                {
                    initialYPosition = PageObject.ParentPage.PageHeight - StampCopy.Height;
                    initialXPosition = PageObject.XPosition + PageObject.Width + 10.0;
                    if(initialXPosition + numberOfCopies * (StampCopy.Width + 5) > PageObject.ParentPage.PageWidth)
                    {
                        initialXPosition = 25.0;
                    }
                }
                var stampCopiesToAdd = new List<ICLPPageObject>();
                for(var i = 0; i < numberOfCopies; i++)
                {
                    var stampCopyClone = StampCopy.Duplicate() as CLPStampCopy;
                    if(stampCopyClone == null)
                    {
                        continue;
                    }
                    stampCopyClone.ParentID = PageObject.UniqueID;
                    stampCopyClone.IsStamped = true;
                    stampCopyClone.Parts = PageObject.Parts;
                    stampCopyClone.IsInternalPageObject = false;
                    stampCopyClone.IsCollectionCopy = IsCollectionStamp;
                    stampCopyClone.CanAcceptPageObjects = IsCollectionStamp;
                    stampCopyClone.PageObjectObjectParentIDs = PageObject.PageObjectObjectParentIDs;
                    stampCopyClone.YPosition = initialYPosition;
                    stampCopyClone.XPosition = initialXPosition;
                    stampCopiesToAdd.Add(stampCopyClone);
                    if(initialXPosition + 2*StampCopy.Width + 5 < PageObject.ParentPage.PageWidth)
                    {
                        initialXPosition += StampCopy.Width + 5;
                    }
                    else if(initialYPosition + 2*StampCopy.Height + 5 < PageObject.ParentPage.PageHeight)
                    {
                        initialXPosition = 25;
                        initialYPosition += StampCopy.Height + 5;
                    }
                }

                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, stampCopiesToAdd);
            }
            else
            {
                MessageBox.Show("What are you counting on the stamp?  Please click the questionmark on the line below the stamp before making copies.", "What are you counting?");
            }  
        }      

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

            var minWidth = IsCollectionStamp ? 75.0 : 50.0;
            const double MIN_HEIGHT = 140.0;
            var newHeight = Height + e.VerticalChange;
            var newWidth = Width + e.HorizontalChange;
            if(newHeight < MIN_HEIGHT)
            {
                newHeight = MIN_HEIGHT;
            }
            if(newWidth < minWidth)
            {
                newWidth = minWidth;
            }
            if(newHeight + YPosition > PageObject.ParentPage.PageHeight)
            {
                newHeight = Height;
            }
            if(newWidth + XPosition > PageObject.ParentPage.PageWidth)
            {
                newWidth = Width;
            }

            //may be able to get away with just calling OnResized() and calling normal ChangePageObjectSize method
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
                var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
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
            if (HasParts() || IsCollectionStamp)
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
                PageObject.CanAcceptStrokes = false;
                StampCopy.SerializedStrokes = StrokeDTO.SaveInkStrokes(clonedStrokes);
                StampCopy.IsStamped = true;
            } 
            else 
            {
                MessageBox.Show("What are you counting on the stamp?  Please click the questionmark on the line below the stamp before making copies.", "What are you counting?");
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Pen;
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
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
                leftBehindStamp.PageObjectObjectParentIDs = new ObservableCollection<string>();

                _originalX = leftBehindStamp.XPosition;
                _originalY = leftBehindStamp.YPosition;

                PageObject.CanAcceptStrokes = false;
                PageObject.CanAcceptPageObjects = false;
                if(PageObject.PageObjectObjectParentIDs.Any())
                {
                    foreach(var pageObject in PageObject.GetPageObjectsOverPageObject())
                    {
                        var clonePageObject = pageObject.Duplicate();

                        // New object stays put, old object leaves, but we shuffle around IDs for ID continuity.
                        PageObject.PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
                        var tempID = pageObject.UniqueID;
                        pageObject.UniqueID = clonePageObject.UniqueID;
                        clonePageObject.UniqueID = tempID;
                        PageObject.PageObjectObjectParentIDs.Add(pageObject.UniqueID);

                        var index = PageObject.ParentPage.PageObjects.IndexOf(PageObject);
                        ACLPPageBaseViewModel.AddPageObjectToPage(clonePageObject, false, false, index);
                        leftBehindStamp.PageObjectObjectParentIDs.Add(clonePageObject.UniqueID);
                    }
                }

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
            if(!HasParts() && !IsCollectionStamp)
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
                var x1 = Double.MaxValue;
                var y1 = Double.MaxValue;
                var x2 = 0.0;
                var y2 = 0.0;
                var copyStrokes = StrokeDTO.LoadInkStrokes(StampCopy.SerializedStrokes);
                foreach(var bounds in copyStrokes.Select(stroke => stroke.GetBounds()))
                {
                    x1 = Math.Min(x1, bounds.Left);
                    y1 = Math.Min(y1, bounds.Top);
                    x2 = Math.Max(x2, bounds.Right);
                    y2 = Math.Max(y2, bounds.Bottom);
                }

                xPosition += x1;
                yPosition += y1;
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

            
            StampCopy.ParentID = PageObject.UniqueID;
            StampCopy.UniqueID = Guid.NewGuid().ToString();
            StampCopy.Parts = PageObject.Parts;
            StampCopy.IsInternalPageObject = false;
            StampCopy.IsCollectionCopy = IsCollectionStamp;
            StampCopy.CanAcceptPageObjects = IsCollectionStamp;
            StampCopy.PageObjectObjectParentIDs = PageObject.PageObjectObjectParentIDs;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                var parentPage = notebookWorkspaceViewModel.Notebook.GetNotebookPageByID(PageObject.ParentPageID);
                var minIndex = parentPage.PageObjects.Count - 1;
                minIndex = StampCopy.GetPageObjectsOverPageObject().Select(pageObject => parentPage.PageObjects.IndexOf(pageObject)).Concat(new[] {minIndex}).Min();

                ACLPPageBaseViewModel.AddPageObjectToPage(parentPage, StampCopy, false, false, minIndex);
                StampCopy.XPosition = xPosition;
                StampCopy.YPosition = yPosition;
            }

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryStampPlace(PageObject.ParentPage, StampCopy.UniqueID));
            StampCopy.OnAdded();
        }

        /// <summary>
        /// Stamp Dragged By Handle
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            IsAdornerVisible = false;
            IsMouseOverShowEnabled = false;

            var newX = PageObject.XPosition + e.HorizontalChange;
            var newY = PageObject.YPosition + e.VerticalChange;
            if(newX < 0)
            {
                newX = 0;
            }
            if(newY < -CLPStamp.HandleHeight)
            {
                newY = -CLPStamp.HandleHeight;
            }
            if(newX > PageObject.ParentPage.PageWidth - PageObject.Width)
            {
                newX = PageObject.ParentPage.PageWidth - PageObject.Width;
            }
            if(newY > PageObject.ParentPage.PageHeight - PageObject.Height + CLPStamp.PartsHeight)
            {
                newY = PageObject.ParentPage.PageHeight - PageObject.Height + CLPStamp.PartsHeight;
            }

            ChangePageObjectPosition(PageObject, newX, newY, false);
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
                var keyPad = new KeypadWindowView("How many things?", 100)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Top = 100,
                        Left = 100
                    };
                keyPad.ShowDialog();
                if(keyPad.DialogResult == true && keyPad.NumbersEntered.Text.Length > 0)
                {
                    var oldParts = PageObject.Parts;
                    var parts = Int32.Parse(keyPad.NumbersEntered.Text);
                    PageObject.Parts = parts;
                    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPartsChanged(PageObject.ParentPage, PageObject.UniqueID, oldParts));
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

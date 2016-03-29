using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NumberLineViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public NumberLineViewModel(NumberLine numberLine)
        {
            PageObject = numberLine;

            ResizeNumberLineCommand = new Command<DragDeltaEventArgs>(OnResizeNumberLineCommandExecute);
            ResizeNumberLineLengthCommand = new Command<DragDeltaEventArgs>(OnResizeNumberLineLengthCommandExecute);
            ResizeStartNumberLineLengthCommand = new Command<DragStartedEventArgs>(OnResizeStartNumberLineLengthCommandExecute);
            ResizeStopNumberLineLengthCommand = new Command<DragCompletedEventArgs>(OnResizeStopNumberLineLengthCommandExecute);
            CheckArrayCompletenessCommand = new Command(OnCheckArrayCompletenessCommandExecute);
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);

            var jumpSizeVisibility = new ToggleRibbonButton("Hide Jump Sizes", "Show Jump Sizes", "pack://application:,,,/Images/Underline16.png", true)
                                     {
                                         IsChecked = !IsJumpSizeLabelsVisible
                                     };
            jumpSizeVisibility.Checked += jumpSizeVisibility_Checked;
            jumpSizeVisibility.Unchecked += jumpSizeVisibility_Checked;
            _contextButtons.Add(jumpSizeVisibility);

            var allowDragging = new ToggleRibbonButton("Disable Drag Arrow", "Enable Drag Arrow", "pack://application:,,,/Images/play.png", true)
                                {
                                    IsChecked = !IsArrowDraggingAllowed
                                };
            allowDragging.Checked += allowDragging_Checked;
            allowDragging.Unchecked += allowDragging_Checked;
            _contextButtons.Add(allowDragging);

            if (NumberLineType == NumberLineTypes.NumberLine)
            {
                _contextButtons.Add(new RibbonButton("Check Number Line", "pack://application:,,,/Resources/Images/Correct32.png", CheckArrayCompletenessCommand, null, true));
            }
        }

        private void jumpSizeVisibility_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            IsJumpSizeLabelsVisible = !(bool)toggleButton.IsChecked;
        }

        private void allowDragging_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            IsArrowDraggingAllowed = !(bool)toggleButton.IsChecked;
        }

        #endregion //Constructor

        #region Model

        [ViewModelToModel("PageObject")]
        public NumberLineTypes NumberLineType
        {
            get { return GetValue<NumberLineTypes>(NumberLineTypeProperty); }
            set { SetValue(NumberLineTypeProperty, value); }
        }

        public static readonly PropertyData NumberLineTypeProperty = RegisterProperty("NumberLineType", typeof(NumberLineTypes));

        [ViewModelToModel("PageObject")]
        public int NumberLineSize
        {
            get { return GetValue<int>(NumberLineSizeProperty); }
            set { SetValue(NumberLineSizeProperty, value); }
        }

        public static readonly PropertyData NumberLineSizeProperty = RegisterProperty("NumberLineSize", typeof (int));

        [ViewModelToModel("PageObject")]
        public bool IsJumpSizeLabelsVisible
        {
            get { return GetValue<bool>(IsJumpSizeLabelsVisibleProperty); }
            set { SetValue(IsJumpSizeLabelsVisibleProperty, value); }
        }

        public static readonly PropertyData IsJumpSizeLabelsVisibleProperty = RegisterProperty("IsJumpSizeLabelsVisible", typeof (bool));

        [ViewModelToModel("PageObject")]
        public bool IsAutoArcsVisible
        {
            get { return GetValue<bool>(IsAutoArcsVisibleProperty); }
            set { SetValue(IsAutoArcsVisibleProperty, value); }
        }

        public static readonly PropertyData IsAutoArcsVisibleProperty = RegisterProperty("IsAutoArcsVisible", typeof(bool));

        [ViewModelToModel("PageObject")]
        public ObservableCollection<NumberLineJumpSize> JumpSizes
        {
            get { return GetValue<ObservableCollection<NumberLineJumpSize>>(JumpSizesProperty); }
            set { SetValue(JumpSizesProperty, value); }
        }

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes", typeof (ObservableCollection<NumberLineJumpSize>));

        [ViewModelToModel("PageObject")]
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks", typeof (ObservableCollection<NumberLineTick>));

        #endregion //Model

        #region Properties

        /// <summary>Whether or not the right-most arrow can be used to change the numberline size while dragging.</summary>
        public bool IsArrowDraggingAllowed
        {
            get { return GetValue<bool>(IsArrowDraggingAllowedProperty); }
            set { SetValue(IsArrowDraggingAllowedProperty, value); }
        }

        public static readonly PropertyData IsArrowDraggingAllowedProperty = RegisterProperty("IsArrowDraggingAllowed", typeof (bool), true);

        #endregion //Properties

        #region Commands

        private double _initialWidth;

        /// <summary>Change the length of the number line</summary>
        public Command<DragDeltaEventArgs> ResizeNumberLineCommand { get; private set; }

        private void OnResizeNumberLineCommandExecute(DragDeltaEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 250;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);

            ChangePageObjectDimensions(PageObject, initialHeight, newWidth);
            PageObject.OnResizing(initialWidth, initialHeight);
        }

        /// <summary>Gets the ResizeStartPageObjectCommand command.</summary>
        public Command<DragStartedEventArgs> ResizeStartNumberLineLengthCommand { get; set; }

        private bool _isClicked;
        private int _oldEnd;

        private void OnResizeStartNumberLineLengthCommandExecute(DragStartedEventArgs e)
        {
            PageObject.ParentPage.History.BeginBatch(new PageObjectResizeBatchHistoryItem(PageObject.ParentPage,
                                                                                          App.MainWindowViewModel.CurrentUser,
                                                                                          PageObject.ID,
                                                                                          new Point(PageObject.Width, PageObject.Height)));
            _initialWidth = Width;
            _isClicked = true;
            _oldEnd = NumberLineSize;
        }

        /// <summary>Gets the ResizeStopPageObjectCommand command.</summary>
        public Command<DragCompletedEventArgs> ResizeStopNumberLineLengthCommand { get; set; }

        private void OnResizeStopNumberLineLengthCommandExecute(DragCompletedEventArgs e)
        {
            var tooLow = false;
            var tooLowNumberTry = 0;
            var initialWidth = Width;
            var initialHeight = Height;
            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is PageObjectResizeBatchHistoryItem)
            {
                ((PageObjectResizeBatchHistoryItem)batch).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();

            if (_isClicked)
            {
                var keyPad = new KeypadWindowView("Change End Number", NumberLine.NUMBER_LINE_MAX_SIZE + 1)
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

                var newNumberLineSize = Int32.Parse(keyPad.NumbersEntered.Text);

                if (newNumberLineSize > NumberLine.NUMBER_LINE_MAX_SIZE)
                {
                    MessageBox.Show("You have entered too large of a number for the endpoint of the number line.");
                    return;
                }

                if (newNumberLineSize <= 0)
                {
                    MessageBox.Show("Number Line must end at at least 1.");
                    return;
                }

                //Number is more than current Number Line Size
                if (newNumberLineSize > NumberLineSize)
                {
                    ChangeNumberLineEndPoints(newNumberLineSize);
                }
                else if (newNumberLineSize < NumberLineSize)
                {
                    var lastMarkedTick = Ticks.Reverse().FirstOrDefault(x => x.IsMarked);
                    if (lastMarkedTick == null ||
                        lastMarkedTick.TickValue <= newNumberLineSize)
                    {
                        ChangeNumberLineEndPoints(newNumberLineSize);
                    }
                    else
                    {
                        MessageBox.Show("You have already drawn past this number on the number line.");
                        tooLow = true;
                        tooLowNumberTry = newNumberLineSize;
                    }
                }
            }
            else
            {
                PageObject.OnResizing(initialWidth, initialHeight);
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            }

            _initialWidth = 0;
            // HACK: Removed for demo.
            //var changeSize = NumberLineSize - _oldEnd;
            //var multiplicationDefinitions = PageObject.ParentPage.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            //var numberLineIDsInHistory = NumberLineAnalysis.GetListOfNumberLineIDsInHistory(PageObject.ParentPage);

            //var tooLowNumber = tooLow ? (int?)tooLowNumberTry : null;

            //foreach (var multiplicationRelationDefinitionTag in multiplicationDefinitions)
            //{
            //    var oldDistanceFromAnswer = _oldEnd - multiplicationRelationDefinitionTag.Product;
            //    var newDistanceFromAnswer = NumberLineSize - multiplicationRelationDefinitionTag.Product;

            //    var tag = new NumberLineDimensionsChangedTag(PageObject.ParentPage,
            //                                                 Origin.StudentPageObjectGenerated,
            //                                                 PageObject.ID,
            //                                                 0,
            //                                                 _oldEnd,
            //                                                 numberLineIDsInHistory.IndexOf(PageObject.ID),
            //                                                 0,
            //                                                 NumberLineSize,
            //                                                 changeSize,
            //                                                 _isClicked,
            //                                                 oldDistanceFromAnswer,
            //                                                 newDistanceFromAnswer,
            //                                                 tooLowNumber);
            //    PageObject.ParentPage.AddTag(tag);
            //}
        }

        /// <summary>Change the length of the number line</summary>
        public Command<DragDeltaEventArgs> ResizeNumberLineLengthCommand { get; private set; }

        private void OnResizeNumberLineLengthCommandExecute(DragDeltaEventArgs e)
        {
            var numberLine = PageObject as NumberLine;

            if (numberLine == null)
            {
                return;
            }

            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 250;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);

            if (!IsArrowDraggingAllowed)
            {
                return;
            }

            if (newWidth >= _initialWidth + numberLine.TickLength)
            {
                _isClicked = false;
                _initialWidth += numberLine.TickLength;
                ChangeNumberLineEndPoints(NumberLineSize + 1);
            }
            else if (newWidth <= _initialWidth - numberLine.TickLength &&
                     NumberLineSize > 5)
            {
                var newNumberLineSize = NumberLineSize - 1;
                var lastMarkedTick = Ticks.Reverse().FirstOrDefault(x => x.IsMarked);
                if (lastMarkedTick == null ||
                    lastMarkedTick.TickValue <= newNumberLineSize)
                {
                    _isClicked = false;
                    _initialWidth -= numberLine.TickLength;
                    ChangeNumberLineEndPoints(newNumberLineSize);
                }
            }

            //TODO: Use for conversion
            //if (_initialWidth + numberLine.TickLength < newWidth && IsArrowDraggingAllowed)
            //{
            //    _isClicked = false;
            //    NumberLineSize++;
            //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
            //                                           new NumberLineEndPointsChangedHistoryItem(PageObject.ParentPage,
            //                                                                                       App.MainWindowViewModel.CurrentUser,
            //                                                                                       PageObject.ID, 0, NumberLineSize - 1));
            //    _initialWidth += numberLine.TickLength;
            //    ChangePageObjectDimensions(PageObject, Height, newWidth);
            //}
        }

        public Command CheckArrayCompletenessCommand { get; private set; }

        private void OnCheckArrayCompletenessCommandExecute()
        {
            var arcs = new List<dynamic>();
            foreach (var jump in JumpSizes)
            {
                arcs.Add(new
                         {
                             Start = jump.StartingTickIndex,
                             End = jump.JumpSize + jump.StartingTickIndex
                         });
            }
            var sortedArcs = arcs.Distinct().OrderBy(x => x.Start).ToList();
            var gaps = 0;
            var overlaps = 0;

            for (var i = 0; i < sortedArcs.Count - 1; i++)
            {
                if (sortedArcs[i].End < sortedArcs[i + 1].Start)
                {
                    gaps++;
                }
                else if(sortedArcs[i].End > sortedArcs[i+1].Start)
                {
                    overlaps++;
                }
            }

            if (gaps > 0 ||
                overlaps > 0)
            {
                var gapsErrorMessage = string.Format("It looks like you have {0} gap(s) between jumps. ", gaps);
                var overlapsErrorMessage = string.Format("It looks like you have {0} overlapping jump(s). ", overlaps);

                var errorMessage = gaps > 0 ? gapsErrorMessage : string.Empty;
                errorMessage += overlaps > 0 ? overlapsErrorMessage : string.Empty;

                MessageBox.Show(errorMessage, "Number Line Check", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Your Number Line looks okay!", "Number Line Check", MessageBoxButton.OK);
            }
        }

        #endregion //Commands

        #region Methods

        public void ChangeNumberLineEndPoints(int newNumberLineEndPoint)
        {
            var numberLine = PageObject as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            var oldHeight = Height;
            var oldNumberLineEndPoint = NumberLineSize;

            numberLine.ChangeNumberLineSize(newNumberLineEndPoint);

            var preStretchedWidth = Width;
            if (Width + XPosition > PageObject.ParentPage.Width)
            {
                Width = PageObject.ParentPage.Width - XPosition;
                PageObject.OnResized(preStretchedWidth, oldHeight);
            }

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new NumberLineEndPointsChangedHistoryItem(PageObject.ParentPage,
                                                                                                 App.MainWindowViewModel.CurrentUser,
                                                                                                 PageObject.ID,
                                                                                                 0,
                                                                                                 0,
                                                                                                 oldNumberLineEndPoint,
                                                                                                 newNumberLineEndPoint,
                                                                                                 preStretchedWidth,
                                                                                                 Width));
        }

        #endregion //Methods

        #region Static Methods

        public static bool InteractWithAcceptedStrokes(NumberLine numberLine, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            if (numberLine == null ||
                !canInteract)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();

            var didInteract = false;
            foreach (var stroke in removedStrokesList.Where(stroke => numberLine.AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                var jumpsRemoved = numberLine.RemoveJumpFromStroke(stroke);
                if (!jumpsRemoved.Any())
                {
                    continue;
                }

                numberLine.AcceptedStrokes.Remove(stroke);
                numberLine.AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());

                var oldHeight = numberLine.Height;
                var oldYPosition = numberLine.YPosition;
                if (!numberLine.JumpSizes.Any() &&
                    numberLine.NumberLineType != NumberLineTypes.AutoArcs)
                {
                    numberLine.Height = numberLine.NumberLineHeight;
                    numberLine.YPosition += (oldHeight - numberLine.Height);
                }

                didInteract = true;

                ACLPPageBaseViewModel.AddHistoryItemToPage(numberLine.ParentPage,
                                                           new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                                                                                                     App.MainWindowViewModel.CurrentUser,
                                                                                                     numberLine.ID,
                                                                                                     new List<Stroke>(),
                                                                                                     new List<Stroke>
                                                                                                     {
                                                                                                         stroke
                                                                                                     },
                                                                                                     new List<NumberLineJumpSize>(),
                                                                                                     jumpsRemoved, 
                                                                                                     oldHeight,
                                                                                                     oldYPosition,
                                                                                                     numberLine.Height,
                                                                                                     numberLine.YPosition));
            }

            foreach (var stroke in addedStrokesList.Where(stroke => numberLine.IsStrokeOverPageObject(stroke) && !numberLine.AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                var jumpsAdded = numberLine.AddJumpFromStroke(stroke);
                if (!jumpsAdded.Any())
                {
                    continue;
                }

                numberLine.AcceptedStrokes.Add(stroke);
                numberLine.AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());

                var oldHeight = numberLine.Height;
                var oldYPosition = numberLine.YPosition;
                    if (numberLine.JumpSizes.Count == 1 &&
                        numberLine.NumberLineType == NumberLineTypes.NumberLine)
                {
                    var tallestPoint = stroke.GetBounds().Top;
                    tallestPoint = tallestPoint - 40;

                    if (tallestPoint < 0)
                    {
                        tallestPoint = 0;
                    }

                    if (tallestPoint > numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight)
                    {
                        tallestPoint = numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight;
                    }

                    numberLine.Height += (numberLine.YPosition - tallestPoint);
                    numberLine.YPosition = tallestPoint;
                }

                didInteract = true;

                ACLPPageBaseViewModel.AddHistoryItemToPage(numberLine.ParentPage,
                                                           new NumberLineJumpSizesChangedHistoryItem(numberLine.ParentPage,
                                                                                                     App.MainWindowViewModel.CurrentUser,
                                                                                                     numberLine.ID,
                                                                                                     new List<Stroke>
                                                                                                     {
                                                                                                         stroke
                                                                                                     },
                                                                                                     new List<Stroke>(),
                                                                                                     jumpsAdded,
                                                                                                     new List<NumberLineJumpSize>(), 
                                                                                                     oldHeight,
                                                                                                     oldYPosition,
                                                                                                     numberLine.Height,
                                                                                                     numberLine.YPosition));
            }

            return didInteract;
        }

        public static void AddNumberLineToPage(CLPPage page)
        {
            var keyPad = new NumberLineCreationView
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

            var numberLineSize = Int32.Parse(keyPad.NumbersEntered.Text);

            var numberLine = new NumberLine(page, numberLineSize, NumberLineTypes.NumberLine);
            ACLPPageBaseViewModel.AddPageObjectToPage(numberLine);
        }

        public static void AddNumberLine2ToPage(CLPPage page)
        {
            var keyPad = new NumberLineCreationView
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

            var numberLineSize = Int32.Parse(keyPad.NumbersEntered.Text);

            var numberLine = new NumberLine(page, numberLineSize, NumberLineTypes.AutoArcs);
            ACLPPageBaseViewModel.AddPageObjectToPage(numberLine);
        }

        #endregion //Static Methods
    }
}
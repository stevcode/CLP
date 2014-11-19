using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
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
            numberLine.CanAcceptStrokes = !IsBackgroundPageObject;
            ResizeNumberLineCommand = new Command<DragDeltaEventArgs>(OnResizeNumberLineCommandExecute);
            ResizeNumberLineLengthCommand = new Command<DragDeltaEventArgs>(OnResizeNumberLineLengthCommandExecute);
            ResizeStartNumberLineLengthCommand = new Command<DragStartedEventArgs>(OnResizeStartNumberLineLengthCommandExecute);
            ResizeStopNumberLineLengthCommand = new Command<DragCompletedEventArgs>(OnResizeStopNumberLineLengthCommandExecute);
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
                (batch as PageObjectResizeBatchHistoryItem).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);

            if (_isClicked)
            {
                var keyPad = new KeypadWindowView("Change End Number", 85)
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

                var newNumberLineSize = Int32.Parse(keyPad.NumbersEntered.Text);

                if (newNumberLineSize >= 85)
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
                    var difference = newNumberLineSize - NumberLineSize;

                    var numberLine = PageObject as NumberLine;

                    if (numberLine == null)
                    {
                        return;
                    }
                    var tickLength = numberLine.TickLength;

                    foreach (var tickNumber in Enumerable.Range(0, difference))
                    {
                        NumberLineSize++;
                    }
                    var oldWidth = Width;
                    var oldHeight = Height;

                    Width += (tickLength * difference);

                    if (Width + XPosition > PageObject.ParentPage.Width)
                    {
                        var oldWidth2 = Width;
                        Width = PageObject.ParentPage.Width - XPosition;
                        PageObject.OnResized(oldWidth2, oldHeight);
                    }
                }
                else if (newNumberLineSize < NumberLineSize)
                {
                    var lastMarkedTick = Ticks.Reverse().FirstOrDefault(x => x.IsMarked);
                    if (lastMarkedTick == null ||
                        lastMarkedTick.TickValue <= newNumberLineSize)
                    {
                        var difference = NumberLineSize - newNumberLineSize;

                        var numberLine = PageObject as NumberLine;
                        if (numberLine == null)
                        {
                            return;
                        }

                        var tickLength = numberLine.TickLength;
                        foreach (var tickNumber in Enumerable.Range(0, difference))
                        {
                            NumberLineSize--;
                        }

                        Width -= (tickLength * difference);
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
            }

            _initialWidth = 0;
            var changeSize = NumberLineSize - _oldEnd;
            var multiplicationDefinitions = PageObject.ParentPage.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            var numberLineIDsInHistory = NumberLineAnalysis.GetListOfNumberLineIDsInHistory(PageObject.ParentPage);

            var tooLowNumber = tooLow ? (int?) tooLowNumberTry : null;

            foreach (var multiplicationRelationDefinitionTag in multiplicationDefinitions)
            {
                var oldDistanceFromAnswer = _oldEnd - multiplicationRelationDefinitionTag.Product;
                var newDistanceFromAnswer = NumberLineSize - multiplicationRelationDefinitionTag.Product;

                var tag = new NumberLineDimensionsChangedTag(PageObject.ParentPage, Origin.StudentPageObjectGenerated, PageObject.ID, 0, _oldEnd, numberLineIDsInHistory.IndexOf(PageObject.ID), 0, NumberLineSize, changeSize, _isClicked, oldDistanceFromAnswer, newDistanceFromAnswer, tooLowNumber);
                PageObject.ParentPage.AddTag(tag);
            }
        }

        /// <summary>Change the length of the number line</summary>
        public Command<DragDeltaEventArgs> ResizeNumberLineLengthCommand { get; private set; }

        private void OnResizeNumberLineLengthCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 250;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);

            var numberLine = PageObject as NumberLine;

            if (numberLine == null)
            {
                return;
            }

            if (_initialWidth + numberLine.TickLength < newWidth && IsArrowDraggingAllowed)
            {
                _isClicked = false;
                numberLine.NumberLineSize++;
                _initialWidth += numberLine.TickLength;
                ChangePageObjectDimensions(PageObject, Height, newWidth);
            }
        }

        #endregion //Commands

        #region Static Methods

        public static void AddNumberLineToPage(CLPPage page)
        {
            var keyPad = new NumberLineCreationView()
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

            var numberLineSize = Int32.Parse(keyPad.NumbersEntered.Text);

            var numberLine = new NumberLine(page, numberLineSize);
            ACLPPageBaseViewModel.AddPageObjectToPage(numberLine);
        }

        #endregion //Static Methods
    }
}
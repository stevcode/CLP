using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
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
        public ObservableCollection<NumberLineJumpSize> JumpSizes
        {
            get { return GetValue<ObservableCollection<NumberLineJumpSize>>(JumpSizesProperty); }
            set { SetValue(JumpSizesProperty, value); }
        }

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes", typeof(ObservableCollection<NumberLineJumpSize>));


        [ViewModelToModel("PageObject")]
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks", typeof (ObservableCollection<NumberLineTick>));

        #endregion //Model


        #region Commands

        private double _initialWidth;

        /// <summary>
        /// Change the length of the number line
        /// </summary>
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

        /// <summary>
        /// Gets the ResizeStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> ResizeStartNumberLineLengthCommand { get; set; }

        private void OnResizeStartNumberLineLengthCommandExecute(DragStartedEventArgs e)
        {
            PageObject.ParentPage.History.BeginBatch(new PageObjectResizeBatchHistoryItem(PageObject.ParentPage,
                                                                                          App.MainWindowViewModel.CurrentUser,
                                                                                          PageObject.ID,
                                                                                          new Point(PageObject.Width, PageObject.Height)));
            _initialWidth = Width;
        }

        /// <summary>
        /// Gets the ResizeStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> ResizeStopNumberLineLengthCommand { get; set; }

        private void OnResizeStopNumberLineLengthCommandExecute(DragCompletedEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is PageObjectResizeBatchHistoryItem)
            {
                (batch as PageObjectResizeBatchHistoryItem).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnResized(initialWidth, initialHeight);
            _initialWidth = 0;
        }

        /// <summary>
        /// Change the length of the number line
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeNumberLineLengthCommand { get; private set; }

        private void OnResizeNumberLineLengthCommandExecute(DragDeltaEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 250;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);


            var numberLine = PageObject as NumberLine;

            if (numberLine == null)
            {
                return;
            }

            if (_initialWidth + numberLine.TickLength < newWidth)
            {
                numberLine.NumberLineSize++;
                _initialWidth += numberLine.TickLength;
                ChangePageObjectDimensions(PageObject, initialHeight, newWidth);    
            }
        }

        #endregion //Commands
    }
}
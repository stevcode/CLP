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
        #region

        public NumberLineViewModel(NumberLine numberLine)
        {
            PageObject = numberLine;
            ResizeNumberLineCommand = new Command<DragDeltaEventArgs>(OnResizeNumberLineCommandExecute);
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
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks", typeof (ObservableCollection<NumberLineTick>));

        #endregion //Model


        #region Commands

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


        #endregion //Commands
    }
}
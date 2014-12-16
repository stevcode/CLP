using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultipleChoiceBoxViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceBoxViewModel(MultipleChoiceBox multipleChoiceBox)
        {
            PageObject = multipleChoiceBox;
            ResizeMultipleChoiceBoxCommand = new Command<DragDeltaEventArgs>(OnResizeMultipleChoiceBoxCommandExecute);
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// List of the available choices for the Multiple Choice Box.
        /// </summary>
        [ViewModelToModel("PageObject")] 
        public List<MultipleChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<List<MultipleChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof(List<MultipleChoiceBubble>)); 

        #endregion //Model

        #region Static Methods

        public static void AddMultipleChoiceBoxToPage(CLPPage page)
        {
            var multipleChoiceBox = new MultipleChoiceBox(page, 4, "C", MultipleChoiceOrientations.Horizontal, MultipleChoiceLabelTypes.Letters);
            ACLPPageBaseViewModel.AddPageObjectToPage(multipleChoiceBox);
        } 

        #endregion //Static Methods

        #region Commands

        /// <summary>Change the length of the number line</summary>
        public Command<DragDeltaEventArgs> ResizeMultipleChoiceBoxCommand { get; private set; }

        private void OnResizeMultipleChoiceBoxCommandExecute(DragDeltaEventArgs e)
        {
            var multipleChoiceBox = PageObject as MultipleChoiceBox;
            if (multipleChoiceBox == null)
            {
                return;
            }
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            var minSize = ChoiceBubbles.Count * multipleChoiceBox.ChoiceBubbleDiameter;

            var newWidth = Math.Max(minSize, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);

            var newHeight = Math.Max(minSize, Height + e.VerticalChange);
            newHeight = Math.Min(newHeight, parentPage.Height - YPosition);

            if (multipleChoiceBox.Orientation == MultipleChoiceOrientations.Horizontal)
            {
                ChangePageObjectDimensions(PageObject, initialHeight, newWidth);
            }
            else
            {
                ChangePageObjectDimensions(PageObject, newHeight, initialWidth);
            }
            
            PageObject.OnResizing(initialWidth, initialHeight);
        } 

        #endregion //Commands
    }
}
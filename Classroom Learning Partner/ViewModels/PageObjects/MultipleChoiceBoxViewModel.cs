using System;
using System.Collections.Generic;
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
    public class MultipleChoiceBoxViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceBoxViewModel(MultipleChoiceBox multipleChoiceBox)
        {
            PageObject = multipleChoiceBox;
            ResizeMultipleChoiceBoxCommand = new Command<DragDeltaEventArgs>(OnResizeMultipleChoiceBoxCommandExecute);
            ChangeCorrectAnswerCommand = new Command(OnChangeCorrectAnswerCommandExecute);

            _contextButtons.Add(MajorRibbonViewModel.Separater);
            _contextButtons.Add(new RibbonButton("Change Correct Answer", "pack://application:,,,/Images/AddToDisplay.png", ChangeCorrectAnswerCommand, null, true));
        }

        #endregion //Constructor

        #region Model

        /// <summary>List of the available choices for the Multiple Choice Box.</summary>
        [ViewModelToModel("PageObject")]
        public List<MultipleChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<List<MultipleChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof (List<MultipleChoiceBubble>));

        #endregion //Model

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

        /// <summary>Changes the Multiple Choice Box's correct answer.</summary>
        public Command ChangeCorrectAnswerCommand { get; private set; }

        private void OnChangeCorrectAnswerCommandExecute()
        {
            var multipleChoiceBox = PageObject as MultipleChoiceBox;
            if (multipleChoiceBox == null)
            {
                return;
            }

            var keyPad = new KeypadWindowView("Index of Correct Answer", 5)
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

            var correctAnswerIndex = Int32.Parse(keyPad.NumbersEntered.Text);
            var correctAnswerValue = MultipleChoiceBubble.IntToUpperLetter(correctAnswerIndex);
            foreach (var multipleChoiceBubble in multipleChoiceBox.ChoiceBubbles)
            {
                multipleChoiceBubble.IsACorrectValue = multipleChoiceBubble.ChoiceBubbleLabel == correctAnswerValue;
            }
        }

        #endregion //Commands

        #region Static Methods

        public static bool InteractWithAcceptedStrokes(MultipleChoiceBox multipleChoiceBox, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            if (multipleChoiceBox == null)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            multipleChoiceBox.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);
            //TODO: Create HistoryItem for this change instead of ObjectsOnPageChanged.
            ACLPPageBaseViewModel.AddHistoryItemToPage(multipleChoiceBox.ParentPage, new ObjectsOnPageChangedHistoryItem(multipleChoiceBox.ParentPage, App.MainWindowViewModel.CurrentUser, addedStrokesList, removedStrokesList));

            MultipleChoiceBubble mostFilledBubble = null;
            var previousStrokeLength = 0;
            foreach (var multipleChoiceBubble in multipleChoiceBox.ChoiceBubbles)
            {
                multipleChoiceBubble.IsMarked = false;

                var bubbleBoundary = new Rect(multipleChoiceBox.XPosition + multipleChoiceBubble.ChoiceBubbleIndex * multipleChoiceBox.ChoiceBubbleGapLength,
                                              multipleChoiceBox.YPosition,
                                              multipleChoiceBox.ChoiceBubbleDiameter,
                                              multipleChoiceBox.ChoiceBubbleDiameter);
                var strokesOverBubble = multipleChoiceBox.AcceptedStrokes.Where(s => s.HitTest(bubbleBoundary, 80));

                var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                if (totalStrokeLength <= previousStrokeLength ||
                    totalStrokeLength <= 100)
                {
                    continue;
                }

                mostFilledBubble = multipleChoiceBubble;
                previousStrokeLength = totalStrokeLength;
            }

            if (mostFilledBubble != null)
            {
                mostFilledBubble.IsMarked = true;
            }

            return true;
        }

        public static void AddMultipleChoiceBoxToPage(CLPPage page)
        {
            var keyPad = new KeypadWindowView("Index of Correct Answer", 5)
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

            var correctAnswerIndex = Int32.Parse(keyPad.NumbersEntered.Text);
            var correctAnswerValue = MultipleChoiceBubble.IntToUpperLetter(correctAnswerIndex);

            var multipleChoiceBox = new MultipleChoiceBox(page, 4, correctAnswerValue, MultipleChoiceOrientations.Horizontal, MultipleChoiceLabelTypes.Letters);
            ACLPPageBaseViewModel.AddPageObjectToPage(multipleChoiceBox);
        }

        #endregion //Static Methods
    }
}
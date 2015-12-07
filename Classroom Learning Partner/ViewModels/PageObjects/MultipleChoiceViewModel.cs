using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultipleChoiceViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceViewModel(MultipleChoice multipleChoice)
        {
            PageObject = multipleChoice;
            ResizeMultipleChoiceBoxCommand = new Command<DragDeltaEventArgs>(OnResizeMultipleChoiceBoxCommandExecute);
            EditCommand = new Command(OnEditCommandExecute);

            _contextButtons.Add(MajorRibbonViewModel.Separater);
            _contextButtons.Add(new RibbonButton("Edit", "pack://application:,,,/Images/AddToDisplay.png", EditCommand, null, true));
        }

        #endregion //Constructor

        #region Model

        /// <summary>List of the available choices for the Multiple Choice Box.</summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<ChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<ObservableCollection<ChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof (ObservableCollection<ChoiceBubble>));

        #endregion //Model

        #region Commands

        /// <summary>Change the length of the number line</summary>
        public Command<DragDeltaEventArgs> ResizeMultipleChoiceBoxCommand { get; private set; }

        private void OnResizeMultipleChoiceBoxCommandExecute(DragDeltaEventArgs e)
        {
            var multipleChoice = PageObject as MultipleChoice;
            if (multipleChoice == null)
            {
                return;
            }
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            var minSize = ChoiceBubbles.Count * multipleChoice.ChoiceBubbleDiameter;

            var newWidth = Math.Max(minSize, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);

            var newHeight = Math.Max(minSize, Height + e.VerticalChange);
            newHeight = Math.Min(newHeight, parentPage.Height - YPosition);

            if (multipleChoice.Orientation == MultipleChoiceOrientations.Horizontal)
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
        public Command EditCommand { get; private set; }

        private void OnEditCommandExecute()
        {
            var multipleChoice = PageObject as MultipleChoice;
            if (multipleChoice == null)
            {
                return;
            }

            var creationViewModel = new MultipleChoiceCreationViewModel(multipleChoice);
            var multiplicationView = new MultipleChoiceCreationView(creationViewModel)
            {
                Owner = Application.Current.MainWindow
            };
            multiplicationView.ShowDialog();

            multipleChoice.SetOffsets();
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

        public static void AddMultipleChoiceToPage(CLPPage page)
        {
            var multipleChoice = new MultipleChoice(page);

            var creationViewModel = new MultipleChoiceCreationViewModel(multipleChoice);
            var multiplicationView = new MultipleChoiceCreationView(creationViewModel)
            {
                Owner = Application.Current.MainWindow
            };
            multiplicationView.ShowDialog();

            if (multiplicationView.DialogResult != true)
            {
                return;
            }

            multipleChoice.SetOffsets();
            ACLPPageBaseViewModel.AddPageObjectToPage(multipleChoice);
        }

        #endregion //Static Methods
    }
}
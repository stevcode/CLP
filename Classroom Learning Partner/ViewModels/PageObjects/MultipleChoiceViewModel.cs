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
using CLP.Entities.Demo;

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
            _contextButtons.Add(new RibbonButton("Edit", "pack://application:,,,/Resources/Images/AddToDisplay.png", EditCommand, null, true));
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

        public static bool InteractWithAcceptedStrokes(MultipleChoice multipleChoice, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            if (multipleChoice == null)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();

            //TODO: Write completely different interaction for point erase situation.
            
            const int threshold = 80;
            var status = ChoiceBubbleStatuses.PartiallyFilledIn;
            var index = -1;
            var isStatusSet = false;
            if (addedStrokesList.Count == 1 &&
                !removedStrokesList.Any())
            {
                var addedStroke = addedStrokesList.First();
                var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(addedStroke);
                if (choiceBubbleStrokeIsOver == null)
                {
                    return false;
                }
                index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                var strokesOverBubble = multipleChoice.StrokesOverChoiceBubble(choiceBubbleStrokeIsOver);
                var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                if (totalStrokeLength >= threshold)
                {
                    status = ChoiceBubbleStatuses.AdditionalFilledIn;
                }
                else
                {
                    totalStrokeLength += addedStroke.StylusPoints.Count;
                    if (totalStrokeLength >= threshold)
                    {
                        status = ChoiceBubbleStatuses.FilledIn;
                        choiceBubbleStrokeIsOver.IsFilledIn = true;
                    }
                    else
                    {
                        status = ChoiceBubbleStatuses.PartiallyFilledIn;
                    }
                }
                isStatusSet = true;
            }

            if (removedStrokesList.Count == 1 &&
                !addedStrokesList.Any())
            {
                var removedStroke = removedStrokesList.First();
                var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(removedStroke);
                if (choiceBubbleStrokeIsOver == null)
                {
                    return false;
                }
                index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                var strokesOverBubble = multipleChoice.StrokesOverChoiceBubble(choiceBubbleStrokeIsOver);
                var isRemovedStrokeOverBubble = strokesOverBubble.FirstOrDefault(s => s.GetStrokeID() == removedStroke.GetStrokeID()) != null;
                if (!isRemovedStrokeOverBubble)
                {
                    // TODO: Log error
                    return false;
                }
                var otherStrokes = strokesOverBubble.Where(s => s.GetStrokeID() != removedStroke.GetStrokeID()).ToList();
                var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                var otherStrokesStrokeLength = otherStrokes.Sum(s => s.StylusPoints.Count);

                if (totalStrokeLength < threshold)
                {
                    status = ChoiceBubbleStatuses.ErasedPartiallyFilledIn;
                }
                else
                {
                    if (otherStrokesStrokeLength < threshold)
                    {
                        status = ChoiceBubbleStatuses.CompletelyErased;
                        choiceBubbleStrokeIsOver.IsFilledIn = false;
                    }
                    else
                    {
                        status = ChoiceBubbleStatuses.IncompletelyErased;
                    }
                }
                isStatusSet = true;
            }

            if (!isStatusSet ||
                index == -1)
            {
                return false;
            }

            multipleChoice.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);
            ACLPPageBaseViewModel.AddHistoryItemToPage(multipleChoice.ParentPage,
                                                       new MultipleChoiceBubbleStatusChangedHistoryItem(multipleChoice.ParentPage,
                                                                                                        App.MainWindowViewModel.CurrentUser,
                                                                                                        multipleChoice,
                                                                                                        index,
                                                                                                        status,
                                                                                                        addedStrokesList,
                                                                                                        removedStrokesList));

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
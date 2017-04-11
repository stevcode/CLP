using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class InterpretationRegionViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public InterpretationRegionViewModel(InterpretationRegion interpretationRegion)
        {
            PageObject = interpretationRegion;

            InitializeCommands();
            InitializeButtons();
        }

        #endregion // Constructor

        #region Buttons

        private void InitializeButtons()
        {
            _contextButtons.Add(MajorRibbonViewModel.Separater);
            _contextButtons.Add(new RibbonButton("Set Expected Value", "pack://application:,,,/Resources/Images/AddToDisplay.png", EditCommand, null, true));

            _toggleIsIntermediaryButton = new ToggleRibbonButton("Is Final", "Is Intermediary", "pack://application:,,,/Resources/Images/AddToDisplay.png", true)
                                          {
                                              IsChecked = IsIntermediary
                                          };
            _toggleIsIntermediaryButton.Checked += toggleIsIntermediaryButton_Checked;
            _toggleIsIntermediaryButton.Unchecked += toggleIsIntermediaryButton_Checked;
            _contextButtons.Add(_toggleIsIntermediaryButton);
        }

        private ToggleRibbonButton _toggleIsIntermediaryButton;

        private void toggleIsIntermediaryButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleRibbonButton;
            if (button == null)
            {
                return;
            }

            IsIntermediary = button.IsChecked != null && (bool)button.IsChecked;
        }

        #endregion // Buttons

        #region Model

        [ViewModelToModel("PageObject")]
        public bool IsIntermediary
        {
            get { return GetValue<bool>(IsIntermediaryProperty); }
            set { SetValue(IsIntermediaryProperty, value); }
        }

        public static readonly PropertyData IsIntermediaryProperty = RegisterProperty("IsIntermediary", typeof(bool));

        [ViewModelToModel("PageObject")]
        public int ExpectedValue
        {
            get { return GetValue<int>(ExpectedValueProperty); }
            set { SetValue(ExpectedValueProperty, value); }
        }

        public static readonly PropertyData ExpectedValueProperty = RegisterProperty("ExpectedValue", typeof(int));

        #endregion // Model

        #region Commands

        private void InitializeCommands()
        {
            EditCommand = new Command(OnEditCommandExecute);
        }

        public Command EditCommand { get; private set; }

        private void OnEditCommandExecute()
        {
            var keyPad = new KeypadWindowView("Expected Value", 9999)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual,
                             NumbersEntered = {
                                                  Text = ExpectedValue.ToString()
                                              }
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            ExpectedValue = int.Parse(keyPad.NumbersEntered.Text);
        }

        #endregion // Commands

        #region Static Methods

        public static bool InteractWithAcceptedStrokes(InterpretationRegion interpretationRegion, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            if (interpretationRegion == null)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();

            interpretationRegion.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);
            ACLPPageBaseViewModel.AddHistoryActionToPage(interpretationRegion.ParentPage,
                                                         new FillInAnswerChangedHistoryAction(interpretationRegion.ParentPage,
                                                                                              App.MainWindowViewModel.CurrentUser,
                                                                                              interpretationRegion,
                                                                                              addedStrokesList,
                                                                                              removedStrokesList));

            return true;
        }

        public static void AddInterpretationRegionToPage(CLPPage page)
        {
            var interpretationRegion = new InterpretationRegion(page);
            ACLPPageBaseViewModel.AddPageObjectToPage(interpretationRegion);
        }

        #endregion //Static Methods
    }
}
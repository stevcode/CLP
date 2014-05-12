using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(RibbonViewModel))]
    public class DisplaysPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaysPanelViewModel" /> class.
        /// </summary>
        public DisplaysPanelViewModel(Notebook notebook)
        {
            InitializeCommands();
            Notebook = notebook;
            Initialized += DisplaysPanelViewModel_Initialized;
            IsVisible = false;
            OnSetSingleDisplayCommandExecute();

            App.MainWindowViewModel.Ribbon.IsProjectorFrozen = true;
        }

        void DisplaysPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
            Location = PanelLocations.Right;
        }

        public override string Title
        {
            get { return "DisplaysPanelVM"; }
        }

        private void InitializeCommands()
        {
            AddGridDisplayCommand = new Command(OnAddGridDisplayCommandExecute);
            AddPageToNewGridDisplayCommand = new Command(OnAddPageToNewGridDisplayCommandExecute);
            SetSingleDisplayCommand = new Command(OnSetSingleDisplayCommandExecute);
            RemoveDisplayCommand = new Command<IDisplay>(OnRemoveDisplayCommandExecute);
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Currently selected <see cref="CLPPage" /> of the <see cref="Notebook" />.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Color of the SingleDisplay background.
        /// </summary>
        public string SingleDisplaySelectedBackgroundColor
        {
            get { return GetValue<string>(SingleDisplaySelectedBackgroundColorProperty); }
            set { SetValue(SingleDisplaySelectedBackgroundColorProperty, value); }
        }

        public static readonly PropertyData SingleDisplaySelectedBackgroundColorProperty = RegisterProperty("SingleDisplaySelectedBackgroundColor", typeof(string));

        /// <summary>
        /// Color of the highlighted border around the SingleDisplay.
        /// </summary>
        public string SingleDisplaySelectedColor
        {
            get { return GetValue<string>(SingleDisplaySelectedColorProperty); }
            set { SetValue(SingleDisplaySelectedColorProperty, value); }
        }

        public static readonly PropertyData SingleDisplaySelectedColorProperty = RegisterProperty("SingleDisplaySelectedColor", typeof(string));

        /// <summary>
        /// The selected display in the list of the Notebook's Displays. Does not include the SingleDisplay.
        /// </summary>
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay), null, OnCurrentDisplayChanged);

        private static void OnCurrentDisplayChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var displayListPanelViewModel = sender as DisplaysPanelViewModel;
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(displayListPanelViewModel == null ||
               notebookWorkspaceViewModel == null ||
               App.CurrentUserMode != App.UserMode.Instructor ||
               args.NewValue == null)
            {
                return;
            }

            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["GrayBorderColor"].ToString();
            displayListPanelViewModel.SingleDisplaySelectedColor = color;
            displayListPanelViewModel.SingleDisplaySelectedBackgroundColor = "Transparent";

            notebookWorkspaceViewModel.CurrentDisplay = null;
            notebookWorkspaceViewModel.CurrentDisplay = args.NewValue as IDisplay;

            if(App.Network.ProjectorProxy == null ||
               notebookWorkspaceViewModel.CurrentDisplay == null)
            {
                return;
            }

            try
            {
                var displayID = notebookWorkspaceViewModel.CurrentDisplay.ID;
                App.Network.ProjectorProxy.SwitchProjectorDisplay(displayID, notebookWorkspaceViewModel.CurrentDisplay.DisplayNumber);
            }
            catch(Exception) { }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Adds a GridDisplay to the notebook.
        /// </summary>
        public Command AddGridDisplayCommand { get; private set; }

        private void OnAddGridDisplayCommandExecute()
        {
            Notebook.AddDisplayToNotebook(new GridDisplay());
            CurrentDisplay = Displays.LastOrDefault();
        }

        /// <summary>
        /// Adds the current page on the SingleDisplay to a new GridDisplay.
        /// </summary>
        public Command AddPageToNewGridDisplayCommand { get; private set; }

        private void OnAddPageToNewGridDisplayCommandExecute()
        {
            var newGridDisplay = new GridDisplay();
            Notebook.AddDisplayToNotebook(newGridDisplay);
            CurrentDisplay = newGridDisplay;
            newGridDisplay.AddPageToDisplay(Notebook.CurrentPage);
        }

        /// <summary>
        /// Sets the current display to the Mirror Display.
        /// </summary>
        public Command SetSingleDisplayCommand { get; private set; }

        private void OnSetSingleDisplayCommandExecute()
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["MainColor"].ToString();
            SingleDisplaySelectedColor = color;
            SingleDisplaySelectedBackgroundColor = App.MainWindowViewModel.Ribbon.IsProjectorFrozen ? "Transparent" : "PaleGreen";
            CurrentDisplay = null;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }
            notebookWorkspaceViewModel.CurrentDisplay = null;

            if(App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                const string DISPLAY_ID = "SingleDisplay";               
                App.Network.ProjectorProxy.SwitchProjectorDisplay(DISPLAY_ID, -1);
            }
            catch(Exception) { }
        }

        /// <summary>
        /// Hides the Display from the list of Displays. Allows permanently deletion if in Authoring Mode.
        /// </summary>
        public Command<IDisplay> RemoveDisplayCommand { get; private set; }

        private void OnRemoveDisplayCommandExecute(IDisplay display)
        {
            var gridDisplay = display as GridDisplay;
            if(gridDisplay != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete Grid Display " + gridDisplay.DisplayNumber + "?", "Delete Display?", MessageBoxButton.YesNo);

                if(result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // TODO: Entities
            //display.IsTrashed = true;
            OnSetSingleDisplayCommandExecute();
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "IsProjectorFrozen" &&
               viewModel is RibbonViewModel)
            {
                if(CurrentDisplay != null ||
                   (viewModel as RibbonViewModel).IsProjectorFrozen)
                {
                    SingleDisplaySelectedBackgroundColor = "Transparent";

                    //take snapshot
                    //send freeze to projector
                }
                else
                {
                    SingleDisplaySelectedBackgroundColor = "PaleGreen";
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods

        #region Static Methods

        public static DisplaysPanelViewModel GetDisplayListPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.DisplaysPanel;
        }

        #endregion //Static Methods
    }
}
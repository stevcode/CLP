﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    public class DisplaysPanelViewModel : APanelBaseViewModel
    {
        private IDataService _dataService;

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="DisplaysPanelViewModel" /> class.</summary>
        public DisplaysPanelViewModel(Notebook notebook, IDataService dataService)
        {
            _dataService = dataService;
            InitializeCommands();
            Notebook = notebook;
            InitializedAsync += DisplaysPanelViewModel_InitializedAsync;
            IsVisible = false;
            OnSetSingleDisplayCommandExecute();
        }

        async Task DisplaysPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
            Location = PanelLocations.Right;
        }

        #endregion //Constructor

        #region Model

        /// <summary>The Model for this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>Currently selected <see cref="CLPPage" /> of the <see cref="Notebook" />.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>A property mapped to a property on the Model Notebook.</summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>));

        #endregion //Model

        #region Bindings

        /// <summary>Color of the SingleDisplay background.</summary>
        public string SingleDisplaySelectedBackgroundColor
        {
            get { return GetValue<string>(SingleDisplaySelectedBackgroundColorProperty); }
            set { SetValue(SingleDisplaySelectedBackgroundColorProperty, value); }
        }

        public static readonly PropertyData SingleDisplaySelectedBackgroundColorProperty = RegisterProperty("SingleDisplaySelectedBackgroundColor", typeof(string));

        /// <summary>Color of the highlighted border around the SingleDisplay.</summary>
        public string SingleDisplaySelectedColor
        {
            get { return GetValue<string>(SingleDisplaySelectedColorProperty); }
            set { SetValue(SingleDisplaySelectedColorProperty, value); }
        }

        public static readonly PropertyData SingleDisplaySelectedColorProperty = RegisterProperty("SingleDisplaySelectedColor", typeof(string));

        /// <summary>The selected display in the list of the Notebook's Displays. Does not include the SingleDisplay.</summary>
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
            if (displayListPanelViewModel == null ||
                notebookWorkspaceViewModel == null ||
                App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher ||
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

            if (App.Network.ProjectorProxy == null ||
                notebookWorkspaceViewModel.CurrentDisplay == null)
            {
                return;
            }

            try
            {
                var displayID = notebookWorkspaceViewModel.CurrentDisplay.ID;
                App.Network.ProjectorProxy.SwitchProjectorDisplay(displayID, notebookWorkspaceViewModel.CurrentDisplay.DisplayNumber);
            }
            catch (Exception) { }
        }

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            AddGridDisplayCommand = new Command(OnAddGridDisplayCommandExecute);
            AddColumnDisplayCommand = new Command(OnAddColumnDisplayCommandExecute);
            AddPageToNewGridDisplayCommand = new Command(OnAddPageToNewGridDisplayCommandExecute);
            SetSingleDisplayCommand = new Command(OnSetSingleDisplayCommandExecute);
            RemoveDisplayCommand = new Command<IDisplay>(OnRemoveDisplayCommandExecute);
        }

        /// <summary>Adds a GridDisplay to the notebook.</summary>
        public Command AddGridDisplayCommand { get; private set; }

        private void OnAddGridDisplayCommandExecute()
        {
            _dataService.AddDisplay(Notebook, new GridDisplay(Notebook));
            CurrentDisplay = Displays.LastOrDefault();
        }

        /// <summary>Adds a ColumnDisplay to the notebook.</summary>
        public Command AddColumnDisplayCommand { get; private set; }

        private void OnAddColumnDisplayCommandExecute()
        {
            _dataService.AddDisplay(Notebook, new ColumnDisplay(Notebook));
            Notebook.CurrentPage = null;
            CurrentDisplay = Displays.LastOrDefault();
        }

        /// <summary>Adds the current page on the SingleDisplay to a new GridDisplay.</summary>
        public Command AddPageToNewGridDisplayCommand { get; private set; }

        private void OnAddPageToNewGridDisplayCommandExecute()
        {
            var newGridDisplay = new GridDisplay();
            _dataService.AddDisplay(Notebook, newGridDisplay);
            CurrentDisplay = newGridDisplay;
            PageHistory.UISleep(1300);
            newGridDisplay.AddPageToDisplay(Notebook.CurrentPage);
        }

        /// <summary>Sets the current display to the Mirror Display.</summary>
        public Command SetSingleDisplayCommand { get; private set; }

        private void OnSetSingleDisplayCommandExecute()
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var color = dict["MainColor"].ToString();
            SingleDisplaySelectedColor = color;
            SingleDisplaySelectedBackgroundColor = App.MainWindowViewModel.IsProjectorFrozen ? "Transparent" : "PaleGreen";
            CurrentDisplay = null;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspaceViewModel == null)
            {
                return;
            }
            notebookWorkspaceViewModel.CurrentDisplay = null;

            if (App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                const string DISPLAY_ID = "SingleDisplay";
                App.Network.ProjectorProxy.SwitchProjectorDisplay(DISPLAY_ID, -1);
            }
            catch (Exception) { }
        }

        /// <summary>Hides the Display from the list of Displays. Allows permanently deletion if in Authoring Mode.</summary>
        public Command<IDisplay> RemoveDisplayCommand { get; private set; }

        private void OnRemoveDisplayCommandExecute(IDisplay display)
        {
            var gridDisplay = display as GridDisplay;
            if (gridDisplay != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete Grid Display " + gridDisplay.DisplayNumber + "?", "Delete Display?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                gridDisplay.IsHidden = true;
            }

            // TODO: Entities
            //display.IsTrashed = true;
            OnSetSingleDisplayCommandExecute();
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsProjectorFrozen" &&
                viewModel is MainWindowViewModel)
            {
                if ((viewModel as MainWindowViewModel).IsProjectorFrozen)
                {
                    SingleDisplaySelectedBackgroundColor = "Transparent";

                    //take snapshot
                    byte[] screenShotByteSource = null;
                    if (CurrentDisplay == null)
                    {
                        var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
                        if (notebookWorkspaceViewModel != null)
                        {
                            var singleDisplayView = notebookWorkspaceViewModel.SingleDisplay.GetFirstView();
                            screenShotByteSource = (singleDisplayView as UIElement).ToImageByteArray();
                        }
                    }
                    else
                    {
                        var displayViewModels = (CurrentDisplay as IModel).GetAllViewModels();
                        foreach (var gridDisplayView in from displayViewModel in displayViewModels
                                                        where displayViewModel is GridDisplayViewModel && (displayViewModel as GridDisplayViewModel).IsDisplayPreview == false
                                                        select displayViewModel.GetFirstView())
                        {
                            screenShotByteSource = (gridDisplayView as UIElement).ToImageByteArray();
                        }
                    }

                    if (screenShotByteSource != null)
                    {
                        var bitmapImage = screenShotByteSource.ToBitmapImage();

                        App.MainWindowViewModel.FrozenDisplayImageSource = bitmapImage;
                    }

                    //send freeze command to projector
                    if (App.Network.ProjectorProxy != null)
                    {
                        try
                        {
                            App.Network.ProjectorProxy.FreezeProjector(true);
                        }
                        catch (Exception) { }
                    }
                }
                else
                {
                    SingleDisplaySelectedBackgroundColor = "PaleGreen";
                    if (App.Network.ProjectorProxy != null)
                    {
                        try
                        {
                            App.Network.ProjectorProxy.FreezeProjector(false);
                        }
                        catch (Exception) { }
                    }
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods
    }
}
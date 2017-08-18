using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class DisplaysPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IRoleService _roleService;

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="DisplaysPanelViewModel" /> class.</summary>
        public DisplaysPanelViewModel(IDataService dataService, IRoleService roleService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => roleService);

            _dataService = dataService;
            _roleService = roleService;

            Notebook = _dataService.CurrentNotebook;

            IsVisible = false;

            InitializeCommands();

            InitializedAsync += DisplaysPanelViewModel_InitializedAsync;
            ClosedAsync += DisplaysPanelViewModel_ClosedAsync;
        }

        #endregion //Constructor

        #region Events

        private Task DisplaysPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;
            Location = PanelLocations.Right;

            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;
            _dataService.CurrentDisplayChanged += _dataService_CurrentDisplayChanged;

            return TaskHelper.Completed;
        }

        private Task DisplaysPanelViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged -= _dataService_CurrentNotebookChanged;
            _dataService.CurrentDisplayChanged -= _dataService_CurrentDisplayChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentNotebookChanged(object sender, EventArgs e)
        {
            Notebook = _dataService.CurrentNotebook;
        }

        private void _dataService_CurrentDisplayChanged(object sender, EventArgs e)
        {
            if (_roleService.Role != ProgramRoles.Teacher)
            {
                return;
            }

            if (App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                if (CurrentDisplay == null)
                {
                    const string DISPLAY_ID = "SingleDisplay";
                    App.Network.ProjectorProxy.SwitchProjectorDisplay(DISPLAY_ID, -1);
                }
                else
                {
                    var displayID = CurrentDisplay.ID;
                    App.Network.ProjectorProxy.SwitchProjectorDisplay(displayID, CurrentDisplay.DisplayNumber);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion // Events

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

        /// <summary>A property mapped to a property on the Model Notebook.</summary>
        [ViewModelToModel("Notebook")]
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay), null);

        #endregion //Model

        #region Commands

        private void InitializeCommands()
        {
            AddGridDisplayCommand = new Command(OnAddGridDisplayCommandExecute);
            AddColumnDisplayCommand = new Command(OnAddColumnDisplayCommandExecute);
            RemoveDisplayCommand = new Command<IDisplay>(OnRemoveDisplayCommandExecute);
        }

        /// <summary>Adds a GridDisplay to the notebook.</summary>
        public Command AddGridDisplayCommand { get; private set; }

        private void OnAddGridDisplayCommandExecute()
        {
            var gridDisplay = new GridDisplay(Notebook);
            _dataService.AddDisplay(Notebook, gridDisplay);
            _dataService.SetCurrentDisplay(gridDisplay);
        }

        /// <summary>Adds a ColumnDisplay to the notebook.</summary>
        public Command AddColumnDisplayCommand { get; private set; }

        private void OnAddColumnDisplayCommandExecute()
        {
            var columnDisplay = new ColumnDisplay(Notebook);
            _dataService.AddDisplay(Notebook, columnDisplay);
            _dataService.SetCurrentDisplay(columnDisplay);
        }

        /// <summary>Hides the Display from the list of Displays. Allows permanently deletion if in Authoring Mode.</summary>
        public Command<IDisplay> RemoveDisplayCommand { get; private set; }

        private void OnRemoveDisplayCommandExecute(IDisplay display)
        {
            var gridDisplay = display as GridDisplay;
            if (gridDisplay != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete Grid Display {gridDisplay.DisplayNumber}?", "Delete Display?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // TODO: If IsAuthoring, completely remove/delete display.
                gridDisplay.IsHidden = true;
            }

            if (CurrentDisplay == display)
            {
                _dataService.SetCurrentDisplay(Displays.FirstOrDefault());
            }
        }

        #endregion //Commands

        #region Methods

        // HACK: Needs to be handeled by some ProjectionService. This was in InterestedIn attribute call from MainWindowViewModel
        protected void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsProjectorFrozen" &&
                viewModel is MainWindowViewModel)
            {
                if ((viewModel as MainWindowViewModel).IsProjectorFrozen)
                {
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
        }

        #endregion //Methods
    }
}
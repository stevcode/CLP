using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SessionsViewModel : ViewModelBase
    {
        #region Fields

        private readonly IDataService _dataService;

        #endregion // Fields

        #region Constructor

        public SessionsViewModel(Notebook notebook, IDataService dataService)
        {
            _dataService = dataService;
            Sessions = DataService.LoadAllSessionsFromZipContainer(notebook.ContainerZipFilePath).ToObservableCollection();
            CurrentSession = Sessions.FirstOrDefault();

            InitializeCommands();
        }

        public SessionsViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Sessions = DataService.LoadAllSessionsFromZipContainer(_dataService.CurrentNotebook.ContainerZipFilePath).ToObservableCollection();
            CurrentSession = Sessions.FirstOrDefault();

            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Session CurrentSession
        {
            get { return GetValue<Session>(CurrentSessionProperty); }
            set { SetValue(CurrentSessionProperty, value); }
        }

        public static readonly PropertyData CurrentSessionProperty = RegisterProperty("CurrentSession", typeof(Session));

        /// <summary>Auto-Mapped property of the Session Model.</summary>
        [ViewModelToModel("CurrentSession")]
        public string SessionTitle
        {
            get { return GetValue<string>(SessionTitleProperty); }
            set { SetValue(SessionTitleProperty, value); }
        }

        public static readonly PropertyData SessionTitleProperty = RegisterProperty("SessionTitle", typeof(string));

        /// <summary>Page Number for the starting page of the session.</summary>
        [ViewModelToModel("CurrentSession")]
        public string StartingPageNumber
        {
            get { return GetValue<string>(StartingPageNumberProperty); }
            set { SetValue(StartingPageNumberProperty, value); }
        }

        public static readonly PropertyData StartingPageNumberProperty = RegisterProperty("StartingPageNumber", typeof(string));

        /// <summary>Comma/Dash page ranges.</summary>
        [ViewModelToModel("CurrentSession")]
        public string PageNumbers
        {
            get { return GetValue<string>(PageNumbersProperty); }
            set { SetValue(PageNumbersProperty, value); }
        }

        public static readonly PropertyData PageNumbersProperty = RegisterProperty("PageNumbers", typeof(string));

        /// <summary>Auto-Mapped property of the Session Model.</summary>
        [ViewModelToModel("CurrentSession")]
        public string SessionComments
        {
            get { return GetValue<string>(SessionCommentsProperty); }
            set { SetValue(SessionCommentsProperty, value); }
        }

        public static readonly PropertyData SessionCommentsProperty = RegisterProperty("SessionComments", typeof(string));

        #endregion // Model

        #region Bindings

        /// <summary>Determines if the View should show or hide the Open Session button.</summary>
        public bool IsOpening
        {
            get { return GetValue<bool>(IsOpeningProperty); }
            set { SetValue(IsOpeningProperty, value); }
        }

        public static readonly PropertyData IsOpeningProperty = RegisterProperty("IsOpening", typeof(bool), false);

        /// <summary>List of all the loaded Sessions for the class.</summary>
        public ObservableCollection<Session> Sessions
        {
            get { return GetValue<ObservableCollection<Session>>(SessionsProperty); }
            set { SetValue(SessionsProperty, value); }
        }

        public static readonly PropertyData SessionsProperty = RegisterProperty("Sessions", typeof(ObservableCollection<Session>), () => new ObservableCollection<Session>());

        /// <summary>Starting Date of the session (mm/dd/yyyy).</summary>
        public string StartingDate
        {
            get { return GetValue<string>(StartingDateProperty); }
            set { SetValue(StartingDateProperty, value); }
        }

        public static readonly PropertyData StartingDateProperty = RegisterProperty("StartingDate", typeof(string), string.Empty);

        /// <summary>Starting Time of the sessions (hh:mm).</summary>
        public string StartingTime
        {
            get { return GetValue<string>(StartingTimeProperty); }
            set { SetValue(StartingTimeProperty, value); }
        }

        public static readonly PropertyData StartingTimeProperty = RegisterProperty("StartingTime", typeof(string), string.Empty);

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            AddSessionCommand = new Command(OnAddSessionCommandExecute);
            EditSessionCommand = new Command(OnEditSessionCommandExecute);
            DeleteSessionCommand = new Command(OnDeleteSessionCommandExecute);
            OpenSessionCommand = new Command(OnOpenSessionCommandExecute);
        }

        /// <summary>Adds a new Session.</summary>
        public Command AddSessionCommand { get; private set; }

        private void OnAddSessionCommandExecute()
        {
            var session = new Session
                          {
                              ContainerZipFilePath = _dataService.CurrentNotebook.ContainerZipFilePath
                          };
            session.NotebookIDs.Add(_dataService.CurrentNotebook.ID);

            var viewModel = this.CreateViewModel<SessionViewModel>(session);
            viewModel.WindowTitle = "New Session";

            var result = viewModel.ShowWindowAsDialog();
            if (result != true)
            {
                return;
            }

            _dataService.SaveSession(session);
            Sessions.Insert(0, session);
            Sessions = Sessions.OrderByDescending(s => s.StartTime).ToObservableCollection();
            CurrentSession = session;
        }

        /// <summary>Edits the selected Session.</summary>
        public Command EditSessionCommand { get; private set; }

        private void OnEditSessionCommandExecute()
        {
            var viewModel = this.CreateViewModel<SessionViewModel>(CurrentSession);
            viewModel.WindowTitle = "Edit Session";

            var result = viewModel.ShowWindowAsDialog();
            if (result != true)
            {
                return;
            }

            _dataService.SaveSession(CurrentSession);
        }

        /// <summary>Deletes the selected Session.</summary>
        public Command DeleteSessionCommand { get; private set; }

        private void OnDeleteSessionCommandExecute()
        {
            Sessions.Remove(CurrentSession);
            CurrentSession = Sessions.FirstOrDefault();
        }

        /// <summary>Opens the selected Session.</summary>
        public Command OpenSessionCommand { get; private set; }

        private async void OnOpenSessionCommandExecute()
        {
            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        #endregion // Commands

        #region Overrides of ViewModelBase

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentSession))
            {
                var selectedSession = e.NewValue as Session;
                if (selectedSession != null)
                {
                    if (selectedSession.StartTime != null)
                    {
                        StartingDate = $"{selectedSession.StartTime:MM/dd/yyyy}";
                        StartingTime = $"{selectedSession.StartTime:HH:mm}";
                    }
                    // TODO: Verify PageNumbers and StartingPageNumber.
                }
                else
                {
                    StartingDate = string.Empty;
                    StartingTime = string.Empty;
                }
            }

            base.OnPropertyChanged(e);
        }

        #endregion
    }
}
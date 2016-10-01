using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
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

        public SessionsViewModel(IDataService dataService)
        {
            _dataService = dataService;

            CurrentSession = new Session();

            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        //[Model]
        public Session CurrentSession
        {
            get { return GetValue<Session>(SessionProperty); }
            set { SetValue(SessionProperty, value); }
        }

        public static readonly PropertyData SessionProperty = RegisterProperty("Session", typeof(Session));

        ///// <summary>Auto-Mapped property of the Session Model.</summary>
        //[ViewModelToModel("CurrentSession")]
        //public string SessionTitle
        //{
        //    get { return GetValue<string>(SessionTitleProperty); }
        //    set { SetValue(SessionTitleProperty, value); }
        //}

        //public static readonly PropertyData SessionTitleProperty = RegisterProperty("SessionTitle", typeof(string));

        ///// <summary>Auto-Mapped property of the Session Model.</summary>
        //[ViewModelToModel("CurrentSession")]
        //public string SessionComments
        //{
        //    get { return GetValue<string>(SessionCommentsProperty); }
        //    set { SetValue(SessionCommentsProperty, value); }
        //}

        //public static readonly PropertyData SessionCommentsProperty = RegisterProperty("SessionComments", typeof(string));

        #endregion // Model

        #region Bindings

        /// <summary>List of all the loaded Sessions for the class.</summary>
        public ObservableCollection<Session> Sessions
        {
            get { return GetValue<ObservableCollection<Session>>(SessionsProperty); }
            set { SetValue(SessionsProperty, value); }
        }

        public static readonly PropertyData SessionsProperty = RegisterProperty("Sessions", typeof(ObservableCollection<Session>), () => new ObservableCollection<Session>());

        /// <summary>Comma/Dash page ranges.</summary>
        public string PageNumbers
        {
            get { return GetValue<string>(PageNumbersProperty); }
            set { SetValue(PageNumbersProperty, value); }
        }

        public static readonly PropertyData PageNumbersProperty = RegisterProperty("PageNumbers", typeof(string), string.Empty);

        /// <summary>Page Number for the starting page of the session.</summary>
        public string StartingPageNumber
        {
            get { return GetValue<string>(StartingPageNumberProperty); }
            set { SetValue(StartingPageNumberProperty, value); }
        }

        public static readonly PropertyData StartingPageNumberProperty = RegisterProperty("StartingPageNumber", typeof(string), string.Empty);

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
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Adds a new Session.</summary>
        public Command AddSessionCommand { get; private set; }

        private void OnAddSessionCommandExecute()
        {
            var session = new Session();
            session.StartTime = DateTime.Now;
            session.SessionTitle = "Blah blah blah";
            Sessions.Add(session);
            CurrentSession = session;
        }

        /// <summary>Validates and confirms changes to the session.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            var combinedDateTimeString = $"{StartingDate} {StartingTime}";
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(combinedDateTimeString);
            }
            catch (Exception)
            {
                MessageBox.Show("Starting Date or Starting Time is not in the correct format.");
                return;
            }

            List<int> actualPageNumbers;
            try
            {
                actualPageNumbers = RangeHelper.ParseStringToIntNumbers(PageNumbers);
            }
            catch (Exception)
            {
                MessageBox.Show("Page Numbers are not in the correct format.");
                return;
            }

            var startingPageNumber = StartingPageNumber.ToInt();
            if (startingPageNumber == null)
            {
                MessageBox.Show("Starting Page Number is not a number.");
                return;
            }

            if (!actualPageNumbers.Contains(startingPageNumber.Value))
            {
                MessageBox.Show("Starting Page Number is not one of the listed Page Numbers.");
                return;
            }

            //CurrentSession.StartTime = dateTime;

            //// If zip container null, save and add to sessions
            //// if not null, just save

            //if (!Sessions.Contains(CurrentSession))
            //{
            //    Sessions.Add(CurrentSession);
            //}
            
            await SaveViewModelAsync();
        }

        /// <summary>Cancels changes to the session.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CancelViewModelAsync();
        }

        #endregion // Commands
    }
}
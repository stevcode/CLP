using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SessionViewModel : ViewModelBase
    {
        #region Fields

        private readonly IDataService _dataService;

        #endregion // Fields

        #region Constructor

        public SessionViewModel(Session session, IDataService dataService)
        {
            _dataService = dataService;

            Session = session;
            if (Session.StartTime != null)
            {
                StartingDate = $"{Session.StartTime:MM/dd/yyyy}";
                StartingTime = $"{Session.StartTime:HH:mm}";
            }
            // TODO: Verify PageNumbers and StartingPageNumber.

            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model]
        public Session Session
        {
            get { return GetValue<Session>(SessionProperty); }
            set { SetValue(SessionProperty, value); }
        }

        public static readonly PropertyData SessionProperty = RegisterProperty("Session", typeof(Session));

        /// <summary>Auto-Mapped property of the Session Model.</summary>
        [ViewModelToModel("Session")]
        public string SessionTitle
        {
            get { return GetValue<string>(SessionTitleProperty); }
            set { SetValue(SessionTitleProperty, value); }
        }

        public static readonly PropertyData SessionTitleProperty = RegisterProperty("SessionTitle", typeof(string));

        /// <summary>Page Number for the starting page of the session.</summary>
        [ViewModelToModel("Session")]
        public string StartingPageNumber
        {
            get { return GetValue<string>(StartingPageNumberProperty); }
            set { SetValue(StartingPageNumberProperty, value); }
        }

        public static readonly PropertyData StartingPageNumberProperty = RegisterProperty("StartingPageNumber", typeof(string));

        /// <summary>Comma/Dash page ranges.</summary>
        [ViewModelToModel("Session")]
        public string PageNumbers
        {
            get { return GetValue<string>(PageNumbersProperty); }
            set { SetValue(PageNumbersProperty, value); }
        }

        public static readonly PropertyData PageNumbersProperty = RegisterProperty("PageNumbers", typeof(string));

        /// <summary>Auto-Mapped property of the Session Model.</summary>
        [ViewModelToModel("Session")]
        public string SessionComments
        {
            get { return GetValue<string>(SessionCommentsProperty); }
            set { SetValue(SessionCommentsProperty, value); }
        }

        public static readonly PropertyData SessionCommentsProperty = RegisterProperty("SessionComments", typeof(string));

        #endregion // Model

        #region Bindings

        /// <summary>Title for the SessionView.</summary>
        public string WindowTitle
        {
            get { return GetValue<string>(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly PropertyData WindowTitleProperty = RegisterProperty("WindowTitle", typeof(string), "Edit Session");

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
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
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

            Session.StartTime = dateTime;

            // TODO: Probably need to do this on viewModel result return.
            Session.PageIDs.Clear();
            var notebook = _dataService.CurrentNotebook;
            var decimalPageNumbers = actualPageNumbers.Select(Convert.ToDecimal).ToList();
            var decimalStartPageNumber = Convert.ToDecimal(startingPageNumber);
            foreach (var page in notebook.Pages)
            {
                if (decimalPageNumbers.Contains(page.PageNumber))
                {
                    Session.PageIDs.Add(page.ID);
                }
                if (decimalStartPageNumber == page.PageNumber)
                {
                    Session.StartingPageID = page.ID;
                }
            }

            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the session.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}
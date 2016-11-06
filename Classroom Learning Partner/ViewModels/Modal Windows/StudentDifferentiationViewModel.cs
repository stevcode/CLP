using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StudentDifferentiationViewModel : ViewModelBase
    {
        #region Constructor

        public StudentDifferentiationViewModel(ClassRoster classRoster)
        {
            ClassRoster = classRoster;

            InitializeCommands();
        }

        #endregion // Constructor

        #region Models

        #region ClassRoster

        /// <summary>Model of this ViewModel.</summary>
        [Model]
        public ClassRoster ClassRoster
        {
            get { return GetValue<ClassRoster>(ClassRosterProperty); }
            set { SetValue(ClassRosterProperty, value); }
        }

        public static readonly PropertyData ClassRosterProperty = RegisterProperty("ClassRoster", typeof (ClassRoster));

        /// <summary>Auto-Mapped property of the Roster Model.</summary>
        [ViewModelToModel("ClassRoster")]
        public ObservableCollection<Person> ListOfStudents
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfStudentsProperty); }
            set { SetValue(ListOfStudentsProperty, value); }
        }

        public static readonly PropertyData ListOfStudentsProperty = RegisterProperty("ListOfStudents", typeof (ObservableCollection<Person>));

        #endregion // ClassRoster

        #endregion // Models

        #region Commands

        private void InitializeCommands()
        {
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Validates and confirms changes to the ClassRoster.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the ClassRoster.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion //Commands
    }
}
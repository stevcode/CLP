using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class PersonViewModel : ViewModelBase
    {
        #region Constructor

        public PersonViewModel(Person person)
        {
            InitializeCommands();

            Person = person;
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model]
        public Person Person
        {
            get { return GetValue<Person>(PersonProperty); }
            set { SetValue(PersonProperty, value); }
        }

        public static readonly PropertyData PersonProperty = RegisterProperty("Person", typeof(Person));

        /// <summary>Person's First Name</summary>
        [ViewModelToModel("Person")]
        public string FirstName
        {
            get { return GetValue<string>(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string));

        /// <summary>Person's nickname or Title, overrides first name in displays.</summary>
        [ViewModelToModel("Person")]
        public string Nickname
        {
            get { return GetValue<string>(NicknameProperty); }
            set { SetValue(NicknameProperty, value); }
        }

        public static readonly PropertyData NicknameProperty = RegisterProperty("Nickname", typeof(string));

        /// <summary>Person's Middle Name</summary>
        [ViewModelToModel("Person")]
        public string MiddleName
        {
            get { return GetValue<string>(MiddleNameProperty); }
            set { SetValue(MiddleNameProperty, value); }
        }

        public static readonly PropertyData MiddleNameProperty = RegisterProperty("MiddleName", typeof(string));

        /// <summary>Person's Last name</summary>
        [ViewModelToModel("Person")]
        public string LastName
        {
            get { return GetValue<string>(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        public static readonly PropertyData LastNameProperty = RegisterProperty("LastName", typeof(string));

        /// <summary>Complete override of DisplayName</summary>
        [ViewModelToModel("Person")]
        public string Alias
        {
            get { return GetValue<string>(AliasProperty); }
            set { SetValue(AliasProperty, value); }
        }

        public static readonly PropertyData AliasProperty = RegisterProperty("Alias", typeof(string));

        /// <summary>Signifies the <see cref="Person" /> is a student.</summary>
        [ViewModelToModel("Person")]
        public bool IsStudent
        {
            get { return GetValue<bool>(IsStudentProperty); }
            set { SetValue(IsStudentProperty, value); }
        }

        public static readonly PropertyData IsStudentProperty = RegisterProperty("IsStudent", typeof(bool));

        #endregion // Model

        #region Bindings

        /// <summary>Title for the PersonView.</summary>
        public string WindowTitle
        {
            get { return GetValue<string>(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly PropertyData WindowTitleProperty = RegisterProperty("WindowTitle", typeof(string), "Edit Person");

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Validates and confirms changes to the person.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            if (!(string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(Alias)))
            {
                await CloseViewModelAsync(true);
            }
            else
            {
                MessageBox.Show("Must have a first name or alias.", "Oops");
            }
        }

        /// <summary>Cancels changes to the person.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}
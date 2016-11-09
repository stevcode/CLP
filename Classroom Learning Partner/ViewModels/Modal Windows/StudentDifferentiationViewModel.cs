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
            GroupCreationViewModel = new GroupCreationViewModel(classRoster, GroupCreationViewModel.GroupTypes.Default);

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

        public static readonly PropertyData ClassRosterProperty = RegisterProperty("ClassRoster", typeof(ClassRoster));

        #endregion // ClassRoster

        #endregion // Models

        #region Bindings

        /// <summary>ViewModel for the GroupCreation Panel.</summary>
        public GroupCreationViewModel GroupCreationViewModel
        {
            get { return GetValue<GroupCreationViewModel>(GroupCreationViewModelProperty); }
            set { SetValue(GroupCreationViewModelProperty, value); }
        }

        public static readonly PropertyData GroupCreationViewModelProperty = RegisterProperty("GroupCreationViewModel", typeof(GroupCreationViewModel));

        #endregion // Bindings

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
            await GroupCreationViewModel.SaveViewModelAsync();
            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the ClassRoster.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await GroupCreationViewModel.CancelViewModelAsync();
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion //Commands
    }
}
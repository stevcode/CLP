using System;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APaneBaseViewModel : ViewModelBase
    {
        protected readonly IDataService _dataService;
        protected readonly IRoleService _roleService;

        #region Constructor

        protected APaneBaseViewModel(IDataService dataService, IRoleService roleService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => roleService);

            _dataService = dataService;
            _roleService = roleService;

            InitializeEventSubscriptions();
        }

        #endregion //Constructor

        #region Events

        private void InitializeEventSubscriptions()
        {
            _roleService.RoleChanged += _roleService_RoleChanged;
        }

        private void _roleService_RoleChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(ResearcherVisibility));
            RaisePropertyChanged(nameof(ResearcherOrTeacherVisibility));
        }

        #endregion // Events

        #region ViewModelBase Overrides

        protected override async Task OnClosingAsync()
        {
            _roleService.RoleChanged -= _roleService_RoleChanged;
            await base.OnClosingAsync();
        }

        #endregion // ViewModelBase Overrides

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public abstract string PaneTitleText { get; }

        #region Visibilities

        public Visibility ResearcherVisibility => _roleService.Role == ProgramRoles.Researcher ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ResearcherOrTeacherVisibility => _roleService.Role == ProgramRoles.Researcher || _roleService.Role == ProgramRoles.Teacher
                                                               ? Visibility.Visible
                                                               : Visibility.Collapsed;

        #endregion // Visibilities

        #endregion //Bindings
    }
}
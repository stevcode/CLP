﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class AMultiDisplayViewModelBase : ViewModelBase
    {
        protected readonly IDataService _dataService;
        protected readonly IRoleService _roleService;

        #region Constructor

        protected AMultiDisplayViewModelBase(IDisplay display, IDataService dataService, IRoleService roleService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => roleService);

            _dataService = dataService;
            _roleService = roleService;

            MultiDisplay = display;

            InitializeEventSubscriptions();
            InitializeCommands();
        }

        #endregion //Constructor

        #region Events

        private void InitializeEventSubscriptions()
        {
            _roleService.RoleChanged += _roleService_RoleChanged;
        }

        private void _roleService_RoleChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsProjectorRole));
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

        #region Model

        /// <summary>The Model for this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public IDisplay MultiDisplay
        {
            get => GetValue<IDisplay>(MultiDisplayProperty);
            private set => SetValue(MultiDisplayProperty, value);
        }

        public static readonly PropertyData MultiDisplayProperty = RegisterProperty("MultiDisplay", typeof(IDisplay));

        /// <summary>Index of the Display in the notebook.</summary>
        [ViewModelToModel("MultiDisplay")]
        public int DisplayNumber
        {
            get => GetValue<int>(DisplayNumberProperty);
            set => SetValue(DisplayNumberProperty, value);
        }

        public static readonly PropertyData DisplayNumberProperty = RegisterProperty("DisplayNumber", typeof(int));

        /// <summary>Pages displayed by the MultiDisplay.</summary>
        [ViewModelToModel("MultiDisplay")]
        public ObservableCollection<CLPPage> Pages
        {
            get => GetValue<ObservableCollection<CLPPage>>(PagesProperty);
            set => SetValue(PagesProperty, value);
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Properties

        /// <summary>Toggle to ignore viewModels of Display Previews</summary>
        public bool IsDisplayPreview
        {
            get => GetValue<bool>(IsDisplayPreviewProperty);
            set => SetValue(IsDisplayPreviewProperty, value);
        }

        public static readonly PropertyData IsDisplayPreviewProperty = RegisterProperty("IsDisplayPreview", typeof(bool), false);

        #endregion //Properties

        #region Bindings

        #region Visibilities

        public bool IsProjectorRole => _roleService.Role == ProgramRoles.Projector;

        public Visibility ResearcherOrTeacherVisibility => _roleService.Role == ProgramRoles.Researcher || _roleService.Role == ProgramRoles.Teacher
                                                               ? Visibility.Visible
                                                               : Visibility.Collapsed;

        #endregion // Visibilities

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            SetPageAsCurrentPageCommand = new Command<CLPPage>(OnSetPageAsCurrentPageCommandExecute);
            RemovePageFromMultiDisplayCommand = new Command<CLPPage>(OnRemovePageFromMultiDisplayCommandExecute);
        }

        /// <summary>Sets the specific page as the notebook's CurrentPage</summary>
        public Command<CLPPage> SetPageAsCurrentPageCommand { get; private set; }

        private void OnSetPageAsCurrentPageCommandExecute(CLPPage page)
        {
            if (_dataService == null ||
                page == null)
            {
                return;
            }

            _dataService.SetCurrentPage(page);
        }

        /// <summary>Removes a specific page from the MultiDisplay.</summary>
        public Command<CLPPage> RemovePageFromMultiDisplayCommand { get; private set; }

        public void OnRemovePageFromMultiDisplayCommandExecute(CLPPage page)
        {
            if (_dataService == null ||
                page == null)
            {
                return;
            }

            _dataService.RemovePageFromCurrentDisplay(page);
        }

        #endregion //Commands
    }
}
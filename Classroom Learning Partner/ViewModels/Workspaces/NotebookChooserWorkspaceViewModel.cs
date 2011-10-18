﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System.IO;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class NotebookChooserWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookChooserWorkspaceViewModel class.
        /// </summary>
        public NotebookChooserWorkspaceViewModel()
        {
            CLPService = new CLPServiceAgent();
            CLPService.ChooseNotebook(this);
        }

        private ICLPServiceAgent CLPService { get; set; }

        private ObservableCollection<NotebookSelectorViewModel> _notebookSelectorViewModels = new ObservableCollection<NotebookSelectorViewModel>();
        public ObservableCollection<NotebookSelectorViewModel> NotebookSelectorViewModels
        {
            get
            {
                return _notebookSelectorViewModels;
            }
        }
    }
}
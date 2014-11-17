﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ProgressPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPanelViewModel" /> class.
        /// </summary>
        public ProgressPanelViewModel(Notebook notebook, StagingPanelViewModel stagingPanel)
        {
            Notebook = notebook;
            Initialized += ProgressPanelViewModel_Initialized;
            StagingPanel = stagingPanel;

            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService != null && notebookService.CurrentClassPeriod != null)
            {
                StudentList = notebookService.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                StudentList = new ObservableCollection<Person>();
                for(int i = 1; i <= 10; i++)
                {
                    StudentList.Add(Person.TestSubmitter);
                }
            }
            
            ClassPeriodsForDisplay = new ObservableCollection<ClassPeriodForDisplay>();

            CurrentPages = Notebook.Pages;

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ChooseClassPeriodCommand = new Command(OnChooseClassPeriodCommandExecute);
            StageStudentNotebookCommand = new Command<Person>(OnStageStudentNotebookCommandExecute);
        }

        void ProgressPanelViewModel_Initialized(object sender, EventArgs e)
        {
            setWidth();
        }

        void setWidth()
        {
            var calculatedWidth = CurrentPages.Count * 40 + 110;
            if(App.Current.MainWindow.ActualWidth < calculatedWidth * 2)
            {
                Length = App.Current.MainWindow.ActualWidth / 2;
            }
            else
            {
                if(calculatedWidth < 200)
                {
                    calculatedWidth = 200;
                }
                Length = calculatedWidth;
            }
        }

        public override string Title
        {
            get { return "ProgressPanelVM"; }
        }

        #endregion //Constructor

        #region Model
        /// <summary>
        /// Notebook associated with the panel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Pages of the Notebook.
        /// </summary>
        public ObservableCollection<CLPPage> CurrentPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(CurrentPagesProperty); }
            set { SetValue(CurrentPagesProperty, value); }
        }

        public static readonly PropertyData CurrentPagesProperty = RegisterProperty("CurrentPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        public ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        public ObservableCollection<ClassPeriodForDisplay> ClassPeriodsForDisplay
        {
            get { return GetValue<ObservableCollection<ClassPeriodForDisplay>>(ClassPeriodsForDisplayProperty); }
            set { SetValue(ClassPeriodsForDisplayProperty, value); }
        }

        public static readonly PropertyData ClassPeriodsForDisplayProperty = RegisterProperty("ClassPeriodsForDisplay", typeof(ObservableCollection<ClassPeriodForDisplay>), () => new ObservableCollection<ClassPeriodForDisplay>());

        /// <summary>
        /// Staging Panel for submissions
        /// </summary>
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel)); 

        #endregion //Bindings

        #region IPanel Override

        /// <summary>
        /// Initial Length of the Panel, before any resizing.
        /// </summary>
        public override double InitialLength
        {
            get { return 400; }
        }

        #endregion //IPanel Override

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            if(page != null)
            {
                //Take thumbnail of page before navigating away from it.
                ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
                Notebook.CurrentPage = page;
            }
        }

        /// <summary>
        /// Appends submissions for the given student/page combo to the staging panel
        /// </summary>
        public Command<Person> StageStudentNotebookCommand { get; private set; }

        private void OnStageStudentNotebookCommandExecute(Person student)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.SetStudentNotebook(student);
        }

        /// <summary>
        /// Meant to allow for looking at progress outside the class period.
        /// TODO: Loading stuff from outside the current class period isn't really supported.
        /// </summary>
        public Command ChooseClassPeriodCommand
        {
            get;
            private set;
        }

        private void OnChooseClassPeriodCommandExecute()
        {
        }

        #endregion
    }
}
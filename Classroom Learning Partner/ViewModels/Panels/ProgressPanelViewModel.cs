﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
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
        public ProgressPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            Initialized += ProgressPanelViewModel_Initialized;

            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                StudentList = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                StudentList = new ObservableCollection<Person>();
                StudentList.Add(Person.TestSubmitter);
            }
            
            ClassPeriodsForDisplay = new ObservableCollection<ClassPeriodForDisplay>();
            ClassPeriod everything = new ClassPeriod();
            List<string> pageIDs = new List<string>();
            foreach(CLPPage page in Notebook.Pages) {
                pageIDs.Add(page.ID);
            }
            everything.PageIDs = pageIDs;
            everything.StartTime = DateTime.Now;
            //TODO: Casey - I removed CurrentClassPeriod in the ClassPeriod.cs class. It was just a temp variable used to create a class period for the first time so that I knew its structure.
            //It never represented the currently loaded class period. App.MainWindowViewModel.CurrentClassPeriod is what you want.
       //     ClassPeriodsForDisplay.Add(new ClassPeriodForDisplay(ClassPeriod.CurrentClassPeriod, false));
            ClassPeriodsForDisplay.Add(new ClassPeriodForDisplay(everything, true));
            SetCurrentPagesFromList(everything.PageIDs);

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ChooseClassPeriodCommand = new Command(OnChooseClassPeriodCommandExecute);
        }

        void ProgressPanelViewModel_Initialized(object sender, EventArgs e)
        {
            setWidth();
        }

        void setWidth()
        {
            var calculatedWidth = CurrentPages.Count * 40 + 90;
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


        #endregion //Bindings

        #region IPanel Override

        #region Overrides of APanelBaseViewModel

        /// <summary>
        /// Initial Length of the Panel, before any resizing.
        /// </summary>
        public override double InitialLength
        {
            get { return 400; }
        }

        #endregion

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
                Notebook.CurrentPage = page;
            }
        }


        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command ChooseClassPeriodCommand
        {
            get;
            private set;
        }

        private void OnChooseClassPeriodCommandExecute()
        {
            
            ClassPeriodChooserView classPeriodChooser = new ClassPeriodChooserView(ClassPeriodsForDisplay);
            classPeriodChooser.Owner = Application.Current.MainWindow;
            classPeriodChooser.ShowDialog();

            if(classPeriodChooser.DialogResult == true)
            {
                List<string> pageIdsToShow = new List<String>();
                foreach(ClassPeriodForDisplay classPeriod in ClassPeriodsForDisplay)
                {
                    if(classPeriod.Showing)
                    {
                        foreach(string pageId in classPeriod.Data.PageIDs)
                        {
                            if(!pageIdsToShow.Contains(pageId))
                            {
                                pageIdsToShow.Add(pageId);
                            }
                        }
                    }
                }
                SetCurrentPagesFromList(pageIdsToShow);
            }
        }

        #endregion

        private void SetCurrentPagesFromList(List<string> PageIDList)
        {
            CurrentPages.Clear();
            foreach(string pageID in PageIDList)
            {
                CLPPage page = Notebook.GetPageByCompositeKeys(pageID, Notebook.OwnerID, 0);
                if(page != null)
                {
                    CurrentPages.Add(page);
                }
                else
                {
                    Logger.Instance.WriteToLog("Page not found: " + pageID);
                }
            }
            setWidth();
        }
    }
}
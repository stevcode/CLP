﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(CLPNotebook notebook)
            : base()
        {
            SetCurrentPageCommand = new Command<MouseButtonEventArgs>(OnSetCurrentPageCommandExecute);
            SetCurrentGridDisplayCommand = new Command<MouseButtonEventArgs>(OnSetCurrentGridDisplayCommandExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute);

            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            Notebook = notebook;
            SubmissionPages = new ObservableCollection<CLPPage>();
            GridDisplays = new ObservableCollection<GridDisplayViewModel>();
            LinkedDisplay = new LinkedDisplayViewModel(Notebook.Pages[0]);
            SelectedDisplay = LinkedDisplay;
            CurrentPage = Notebook.Pages[0];

            if(App.CurrentUserMode == App.UserMode.Instructor)
            {
                SelectedDisplay.IsOnProjector = true;
                WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
            }
            else
            {
                SelectedDisplay.IsOnProjector = false;
            }

            Notebook.GeneratePageIndexes();

            FilteredSubmissions = new CollectionViewSource();
            FilterTypes = new ObservableCollection<string>();
            FilterTypes.Add("Student Name - Ascending");
            FilterTypes.Add("Student Name - Descending");
            FilterTypes.Add("Time In - Ascending");
            FilterTypes.Add("Time In - Descending");
        }

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }

        

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        #region Model

        /// <summary>
        /// Model
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        /// <summary>
        /// Notebook Model Property
        /// </summary>
        [ViewModelToModel("Notebook","Pages")]
        public ObservableCollection<CLPPage> NotebookPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(NotebookPagesProperty); }
            set { SetValue(NotebookPagesProperty, value); }
        }

        public static readonly PropertyData NotebookPagesProperty = RegisterProperty("NotebookPages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Bindings

        #region Displays

        /// <summary>
        /// Collection of all available GridDisplays.
        /// </summary>
        public ObservableCollection<GridDisplayViewModel> GridDisplays
        {
            get { return GetValue<ObservableCollection<GridDisplayViewModel>>(GridDisplaysProperty); }
            set { SetValue(GridDisplaysProperty, value); }
        }

        public static readonly PropertyData GridDisplaysProperty = RegisterProperty("GridDisplays", typeof(ObservableCollection<GridDisplayViewModel>));

        /// <summary>
        /// The MirrorDisplay of the Notebook.
        /// </summary>
        public LinkedDisplayViewModel LinkedDisplay
        {
            get { return GetValue<LinkedDisplayViewModel>(LinkedDisplayProperty); }
            private set { SetValue(LinkedDisplayProperty, value); }
        }

        /// <summary>
        /// Register the LinkedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LinkedDisplayProperty = RegisterProperty("LinkedDisplay", typeof(LinkedDisplayViewModel));

        /// <summary>
        /// The Currently Selected Display.
        /// </summary>
        public IDisplayViewModel SelectedDisplay
        {
            get { return GetValue<IDisplayViewModel>(SelectedDisplayProperty); }
            set
            {
                SetValue(SelectedDisplayProperty, value);
                if (SelectedDisplay != null)
                {
                    if (SelectedDisplay.IsOnProjector)
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
                    }
                    else
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
                    }
                }
            }
        }

        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(IDisplayViewModel));

        /// <summary>
        /// Color of Dispaly Background.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        #endregion //Displays

        #region Submissions SideBar

        /// <summary>
        /// All the submissions for the desired page.
        /// </summary>
        public ObservableCollection<CLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionPagesProperty); }
            set 
            { 
                SetValue(SubmissionPagesProperty, value);
                SelectedFilterType = "Student Name - Ascending"; 
                FilterSubmissions("Student Name - Ascending");
            } 
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource FilteredSubmissions
        {
            get { return GetValue<CollectionViewSource>(FilteredSubmissionsProperty); }
            set { SetValue(FilteredSubmissionsProperty, value); }
        }

        public static readonly PropertyData FilteredSubmissionsProperty = RegisterProperty("FilteredSubmissions", typeof(CollectionViewSource), null);

        /// <summary>
        /// Types of Filters for sorting SubmissionPages
        /// STEVE - Change to PageTags. All PageTags should be sortable/filterable.
        /// </summary>
        public ObservableCollection<string> FilterTypes
        {
            get { return GetValue<ObservableCollection<string>>(FilterTypesProperty); }
            set { SetValue(FilterTypesProperty, value); }
        }

        public static readonly PropertyData FilterTypesProperty = RegisterProperty("FilterTypes", typeof(ObservableCollection<string>), null);

        /// <summary>
        /// The Current SubmissionPages Filter.
        /// </summary>
        public string SelectedFilterType
        {
            get { return GetValue<string>(SelectedFilterTypeProperty); }
            set
            {
                SetValue(SelectedFilterTypeProperty, value);
                FilterSubmissions(SelectedFilterType);
            }
        }

        public static readonly PropertyData SelectedFilterTypeProperty = RegisterProperty("SelectedFilterType", typeof(string), null);

        #endregion Submissions SideBar

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }

            set
            {
                SetValue(CurrentPageProperty, value);
                SelectedDisplay.AddPageToDisplay(value);
            }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Bindings

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetCurrentPageCommand command is executed.
        /// </summary>
        private void OnSetCurrentPageCommandExecute(MouseButtonEventArgs e)
        {
            CurrentPage = ((e.Source as CLPPagePreviewView).ViewModel as CLPPageViewModel).Page;
        }

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentGridDisplayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetCurrentPageCommand command is executed.
        /// </summary>
        private void OnSetCurrentGridDisplayCommandExecute(MouseButtonEventArgs e)
        {
            SelectedDisplay = ((e.Source as ItemsControl).DataContext as GridDisplayViewModel);
        }

        /// <summary>
        /// Gets the MakePageLongerCommand command.
        /// </summary>
        public Command MakePageLongerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the MakePageLongerCommand command is executed.
        /// </summary>
        private void OnMakePageLongerCommandExecute()
        {
            if((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
            {
                CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
                page.PageHeight += 200;
                ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ResizePage();

                double yDifference = page.PageHeight - CLPPage.LANDSCAPE_HEIGHT;

                double times = yDifference / 200;

                Logger.Instance.WriteToLog("[METRICS]: PageLength Increased " + times + " times on page " + page.PageIndex);
            }
        }

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsAuthoring")
            {                
                SelectedDisplay = LinkedDisplay;
                if ((viewModel as MainWindowViewModel).IsAuthoring)
                {
                    SelectedDisplay.IsOnProjector = false;
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;
                }
                else
                {
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Collapsed;
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
            
        }

        public void FilterSubmissions(string Sort)
        {
            FilteredSubmissions = new CollectionViewSource();
            FilteredSubmissions.Source = SubmissionPages;
            FilteredSubmissions.SortDescriptions.Clear();

            PropertyGroupDescription gd = new PropertyGroupDescription();
            gd.PropertyName = "SubmitterName";

            if(Sort == "Student Name - Ascending")
            {
                FilteredSubmissions.GroupDescriptions.Add(gd);
                SortDescription sdAA = new SortDescription("SubmitterName", ListSortDirection.Ascending);
                FilteredSubmissions.SortDescriptions.Add(sdAA);
            }
            else if(Sort == "Student Name - Descending")
            {
                FilteredSubmissions.GroupDescriptions.Add(gd);
                SortDescription sdAD = new SortDescription("SubmitterName", ListSortDirection.Descending);
                FilteredSubmissions.SortDescriptions.Add(sdAD);
            }
            else if(Sort == "Time In - Ascending")
            {
                SortDescription sdTA = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
                FilteredSubmissions.SortDescriptions.Add(sdTA);
            }
            else if(Sort == "Time In - Descending")
            {
                SortDescription sdTD = new SortDescription("SubmissionTime", ListSortDirection.Descending);
                FilteredSubmissions.SortDescriptions.Add(sdTD);
            }
        }

        #endregion //Methods

    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
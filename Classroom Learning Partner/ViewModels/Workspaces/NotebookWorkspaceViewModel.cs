using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
using Classroom_Learning_Partner.Resources;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    [InterestedIn(typeof(RibbonViewModel))]
    [InterestedIn(typeof(HoverBoxViewModel))]
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

            WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
            Notebook = notebook;
            NotebookPagesPanel = new NotebookPagesPanelViewModel(notebook);
            LeftPanel = NotebookPagesPanel;
            SubmissionPages = new ObservableCollection<CLPPage>();
            GridDisplays = new ObservableCollection<GridDisplayViewModel>();
            LinkedDisplay = new LinkedDisplayViewModel(Notebook.Pages[0]);
            SelectedDisplay = LinkedDisplay;
            CurrentPage = Notebook.Pages[0];
            StudentsWithNoSubmissions = getStudentsWithNoSubmissions();
            TopThumbnailsVisible = App.MainWindowViewModel.Ribbon.ThumbnailsTop;
            SideThumbnailsVisible = !TopThumbnailsVisible;


            if(App.CurrentUserMode == App.UserMode.Instructor)
            {
                SelectedDisplay.IsOnProjector = true;
                WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
            }
            else if (App.CurrentUserMode == App.UserMode.Projector)
            {
                IsSideBarVisible = false;
            }
            else
            {
                SelectedDisplay.IsOnProjector = false;
            }

            Notebook.GeneratePageIndexes();

            FilteredSubmissions = new CollectionViewSource();
            FilterTypes = new ObservableCollection<string>();
            FilterTypes.Add("Student Name - Alphabetical");
            FilterTypes.Add("Group Submissions");
            FilterTypes.Add("Submissions By Group Name");
            FilterTypes.Add("Time In - Ascending");
            FilterTypes.Add("Time In - Descending");

            ObservableCollection<Tag> tags = getAllTags(Notebook.Pages);

     /**       foreach(Tag t in tags)
            {
                if(t.TagType != null)
                {
                    FilterTypes.Add(t.TagType.Name);
                }
            }   
            */
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
                        WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
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

        /// <summary>
        /// Top Thumbnail Toggle
        /// </summary>
        public Boolean TopThumbnailsVisible
        {
            get { return GetValue<Boolean>(TopThumbnailsVisibleProperty); }
            set { SetValue(TopThumbnailsVisibleProperty, value); }
        }

        public static readonly PropertyData TopThumbnailsVisibleProperty = RegisterProperty("TopThumbnailsVisible", typeof(Boolean));

        /// <summary>
        /// Side Thumbnail Toggle
        /// </summary>
        public Boolean SideThumbnailsVisible
        {
            get { return GetValue<Boolean>(SideThumbnailsVisibleProperty); }
            set { SetValue(SideThumbnailsVisibleProperty, value); }
        }

        public static readonly PropertyData SideThumbnailsVisibleProperty = RegisterProperty("SideThumbnailsVisible", typeof(Boolean));


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
                SelectedFilterType = "Student Name - Alphabetical";
                //FilterSubmissions("Student Name - Alphabetical");
            } 
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>));


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string StudentsWithNoSubmissions
        {
            get { return GetValue<string>(StudentsWithNoSubmissionsProperty); }
            set { SetValue(StudentsWithNoSubmissionsProperty, value); }
        }

        public static readonly PropertyData StudentsWithNoSubmissionsProperty = RegisterProperty("StudentsWithNoSubmissions", typeof(string), "");

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
        /// Gets or sets the property value.
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

        #region Panels

        /// <summary>
        /// Right side Panel.
        /// </summary>
        public IPanel RightPanel
        {
            get { return GetValue<IPanel>(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        public static readonly PropertyData RightPanelProperty = RegisterProperty("RightPanel", typeof(IPanel), null);

        /// <summary>
        /// Left side Panel.
        /// </summary>
        public IPanel LeftPanel
        {
            get { return GetValue<IPanel>(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        public static readonly PropertyData LeftPanelProperty = RegisterProperty("LeftPanel", typeof(IPanel), null);

        /// <summary>
        /// NotebookPagesPanel.
        /// </summary>
        public NotebookPagesPanelViewModel NotebookPagesPanel
        {
            get { return GetValue<NotebookPagesPanelViewModel>(NotebookPagesPanelProperty); }
            set { SetValue(NotebookPagesPanelProperty, value); }
        }

        public static readonly PropertyData NotebookPagesPanelProperty = RegisterProperty("NotebookPagesPanel", typeof(NotebookPagesPanelViewModel), null);

        /// <summary>
        /// DisplayPanel.
        /// </summary>
        public DisplayListPanelViewModel DisplayListPanel
        {
            get { return GetValue<DisplayListPanelViewModel>(DisplayListPanelProperty); }
            set { SetValue(DisplayListPanelProperty, value); }
        }

        public static readonly PropertyData DisplayListPanelProperty = RegisterProperty("DisplayListPanel", typeof(DisplayListPanelViewModel), new DisplayListPanelViewModel());

        /// <summary>
        /// HistoryPanel.
        /// </summary>
        public ObservableCollection<CLPPage> HistoryPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(HistoryPagesProperty); }
            set { SetValue(HistoryPagesProperty, value); }
        }

        public static readonly PropertyData HistoryPagesProperty = RegisterProperty("HistoryPages", typeof(ObservableCollection<CLPPage>));

        #endregion //Panels

        #endregion //Bindings

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(MouseButtonEventArgs e)
        {
            CurrentPage = ((e.Source as CLPPagePreviewView).ViewModel as CLPPageViewModel).Page;
            SetHistoryPages();
        }

        /// <summary>
        /// Gets the SetCurrentPageCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> SetCurrentGridDisplayCommand { get; private set; }

        private void OnSetCurrentGridDisplayCommandExecute(MouseButtonEventArgs e)
        {
            SelectedDisplay = ((e.Source as ItemsControl).DataContext as GridDisplayViewModel);
        }

        /// <summary>
        /// Gets the MakePageLongerCommand command.
        /// </summary>
        public Command MakePageLongerCommand { get; private set; }

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
                    WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Collapsed;
                }
            }
            if(propertyName == "IsUnknown" || propertyName == "IsCorrect" || propertyName == "IsIncorrect" || propertyName == "IsStarred")
            {
                System.Console.WriteLine("changed");
                //FilterSubmissions(SelectedFilterType);
            }
            if (propertyName == "SideBarVisibility")
            {
                IsSideBarVisible = (viewModel as RibbonViewModel).SideBarVisibility;
            }

            if(propertyName == "ThumbnailsTop")
            {
                TopThumbnailsVisible = (viewModel as RibbonViewModel).ThumbnailsTop;
                IsSideBarVisible = !TopThumbnailsVisible;
            }
            base.OnViewModelPropertyChanged(viewModel, propertyName);
            
        }
        public string getStudentsWithNoSubmissions()
        {
            ObservableCollection<string> UserNames = new ObservableCollection<string>();
            //Steve - move to CLPService and grab from database
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";

            if(File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                string name;
                while(!((name = reader.ReadLine()) == null))
                {
                    string user = name.Split(new char[] { ',' })[0];
                    UserNames.Add(user);
                }
                reader.Dispose();
            }
            foreach(CLPPage p in SubmissionPages) {
                UserNames.Remove(p.SubmitterName);

            }
            string names = "";
            foreach(string user in UserNames)
            {
                names = names + user + "\n";
            }
            names = names.Substring(0, names.Length - 2);
            return names;
        }





        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsSideBarVisible
        {
            get { return GetValue<bool>(IsSideBarVisibleProperty); }
            set { SetValue(IsSideBarVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideBarVisibleProperty = RegisterProperty("IsSideBarVisible", typeof(bool), true);

        public void OnlyGroupSubmissionsFilter(object sender, FilterEventArgs e)
        {
            CLPPage page = e.Item as CLPPage;
            if(page != null)
            {
                if(page.IsGroupSubmission == true)
                {
                    e.Accepted = true;
                }
                else {
                    e.Accepted = false;
                }
            }
        }

        public void OnlyIndividualSubmissionsFilter(object sender, FilterEventArgs e)
        {
            CLPPage page = e.Item as CLPPage;
            if(page != null)
            {
                if(page.IsGroupSubmission == true)
                {
                    e.Accepted = false;
                }
                else
                {
                    e.Accepted = true;
                }
            }
        }

        public ObservableCollection<Tag>  getAllTags(ObservableCollection<CLPPage> pages) 
        {
            ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
            foreach (CLPPage page in pages) {
                foreach(Tag tag in page.PageTags)
                {
                    if(!tags.Contains(tag))
                    {
                        tags.Add(tag);

                    }
                }
            }
            return tags;
        }



        public void FilterSubmissions(string Sort)
        {
            FilteredSubmissions = new CollectionViewSource();     
            FilteredSubmissions.Source = SubmissionPages;
            StudentsWithNoSubmissions = getStudentsWithNoSubmissions();

            PropertyGroupDescription submitterNameDescription = new PropertyGroupDescription("SubmitterName");
            PropertyGroupDescription groupNameDescription = new PropertyGroupDescription("GroupName", new GroupLabelConverter());
            PropertyGroupDescription timeDescription = new PropertyGroupDescription("SubmissionTime");
            PropertyGroupDescription isGroupDescription = new PropertyGroupDescription("IsGroupSubmission", new BooleantoGroupConverter());
            PropertyGroupDescription correctnessDescription = new PropertyGroupDescription(null, new PagetToCorrectnessTagConverter());
            PropertyGroupDescription starredDescription = new PropertyGroupDescription(null, new PagetToStarredTagConverter());

            SortDescription submitterNameSort = new SortDescription("SubmitterName", ListSortDirection.Ascending);
            SortDescription groupNameSort = new SortDescription("GroupName", ListSortDirection.Ascending);
            SortDescription timeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);
            SortDescription timeAscendingSort = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
             SortDescription isGroupSubmissionSort = new SortDescription("IsGroupSubmission", ListSortDirection.Ascending);

            if(Sort == "Student Name - Alphabetical")
            {
               FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
               FilteredSubmissions.SortDescriptions.Add(submitterNameSort);
            }

            else if(Sort == "Group Submissions")
            {
                FilteredSubmissions.Filter += new FilterEventHandler(OnlyGroupSubmissionsFilter);
   
                FilteredSubmissions.GroupDescriptions.Add(groupNameDescription);  
                FilteredSubmissions.SortDescriptions.Add(groupNameSort);
                FilteredSubmissions.GroupDescriptions.Add(timeDescription);
              
                FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
                FilteredSubmissions.SortDescriptions.Add(submitterNameSort);
                

            }
            else if(Sort == "Submissions By Group Name")
            {
                FilteredSubmissions.Filter += new FilterEventHandler(OnlyIndividualSubmissionsFilter);

                FilteredSubmissions.GroupDescriptions.Add(groupNameDescription);
                FilteredSubmissions.SortDescriptions.Add(groupNameSort);

                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
                FilteredSubmissions.SortDescriptions.Add(submitterNameSort);
            }

            else if(Sort == "Time In - Ascending")
            {
              
                FilteredSubmissions.GroupDescriptions.Add(timeDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);

                FilteredSubmissions.SortDescriptions.Add(timeAscendingSort);
            }
            else if(Sort == "Time In - Descending")
            {
                FilteredSubmissions.GroupDescriptions.Add(timeDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);


                FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
            }
            else if(Sort == "Correctness")
            {
                FilteredSubmissions.GroupDescriptions.Clear();
                FilteredSubmissions.GroupDescriptions.Add(correctnessDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            }
            else if(Sort == "Starred")
            {
                FilteredSubmissions.GroupDescriptions.Clear();
                FilteredSubmissions.GroupDescriptions.Add(starredDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            }
        }

        public void SetHistoryPages()
        {
            string id = CurrentPage.UniqueID;
            System.Collections.ObjectModel.ObservableCollection<CLPPage> pages;
            if(Notebook.Submissions.ContainsKey(id))
            {
                pages = Notebook.Submissions[id];
            }
            else
            {
                Notebook.Submissions.Add(id, new System.Collections.ObjectModel.ObservableCollection<CLPPage>());
                pages = Notebook.Submissions[id];
            }
            HistoryPages = pages;

        }

        #endregion //Methods

    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
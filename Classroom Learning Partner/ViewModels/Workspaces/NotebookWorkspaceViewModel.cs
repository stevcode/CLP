using System;
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
        {
            Notebook = notebook;
            SelectedDisplay = MirrorDisplay;

            NotebookPagesPanel = new NotebookPagesPanelViewModel(notebook);
            LeftPanel = NotebookPagesPanel;
            DisplayListPanel = new DisplayListPanelViewModel(notebook);
            RightPanel = DisplayListPanel;



            WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
            StudentsWithNoSubmissions = getStudentsWithNoSubmissions();

            //Notebook.GeneratePageIndexes(); //TODO: re-add GeneratePageIndexes for PageIndex return

            #region Tag Stuff

            FilteredSubmissions = new CollectionViewSource();
            FilterTypes = new ObservableCollection<string>();
            FilterTypes.Add("Student Name - Alphabetical");
            FilterTypes.Add("Group Submissions");
            FilterTypes.Add("Submissions By Group Name");
            FilterTypes.Add("Time In - Ascending");
            FilterTypes.Add("Time In - Descending");

            ObservableCollection<Tag> tags = getAllTags(Notebook.Pages);

            foreach(Tag t in tags)
            {
                if(t.TagType != null)
                {
                    FilterTypes.Add(t.TagType.Name);
                }
            }

            // Just hardcode in some tag types for now
            FilterTypes.Add(RepresentationCorrectnessTagType.Instance.Name);
            FilterTypes.Add(ArrayXAxisStrategyTagType.Instance.Name);
            FilterTypes.Add(ArrayYAxisStrategyTagType.Instance.Name);
            FilterTypes.Add(ArrayDivisionCorrectnessTagType.Instance.Name);
            FilterTypes.Add(ArrayHorizontalDivisionsTagType.Instance.Name);
            FilterTypes.Add(ArrayVerticalDivisionsTagType.Instance.Name);
            FilterTypes.Add(ArrayOrientationTagType.Instance.Name);
            FilterTypes.Add(StampPartsPerStampTagType.Instance.Name);
            FilterTypes.Add(StampGroupingTypeTagType.Instance.Name);

            #endregion //Tag Stuff

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
        public ObservableCollection<ICLPPage> NotebookPages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(NotebookPagesProperty); }
            set { SetValue(NotebookPagesProperty, value); }
        }

        public static readonly PropertyData NotebookPagesProperty = RegisterProperty("NotebookPages", typeof(ObservableCollection<ICLPPage>));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPMirrorDisplay MirrorDisplay
        {
            get { return GetValue<CLPMirrorDisplay>(MirrorDisplayProperty); }
            set { SetValue(MirrorDisplayProperty, value); }
        }

        public static readonly PropertyData MirrorDisplayProperty = RegisterProperty("MirrorDisplay", typeof(CLPMirrorDisplay));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<ICLPDisplay> Displays
        {
            get { return GetValue<ObservableCollection<ICLPDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<ICLPDisplay>));

        #endregion //Model

        #region Bindings

        #region Displays

        /// <summary>
        /// The Currently Selected Display.
        /// </summary>
        public ICLPDisplay SelectedDisplay
        {
            get { return GetValue<ICLPDisplay>(SelectedDisplayProperty); }
            set { SetValue(SelectedDisplayProperty, value); }
        }

        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(ICLPDisplay));

        /// <summary>
        /// Color of Workspace Background.
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
        public ObservableCollection<ICLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(SubmissionPagesProperty); }
            set 
            { 
                SetValue(SubmissionPagesProperty, value);
                SelectedFilterType = "Student Name - Alphabetical";
                //FilterSubmissions("Student Name - Alphabetical");
            } 
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<ICLPPage>), () => new ObservableCollection<ICLPPage>());


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

        #region Panels

        /// <summary>
        /// Right side Panel.
        /// </summary>
        public IPanel RightPanel
        {
            get { return GetValue<IPanel>(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        public static readonly PropertyData RightPanelProperty = RegisterProperty("RightPanel", typeof(IPanel));

        /// <summary>
        /// Left side Panel.
        /// </summary>
        public IPanel LeftPanel
        {
            get { return GetValue<IPanel>(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        public static readonly PropertyData LeftPanelProperty = RegisterProperty("LeftPanel", typeof(IPanel));

        /// <summary>
        /// NotebookPagesPanel.
        /// </summary>
        public NotebookPagesPanelViewModel NotebookPagesPanel
        {
            get { return GetValue<NotebookPagesPanelViewModel>(NotebookPagesPanelProperty); }
            set { SetValue(NotebookPagesPanelProperty, value); }
        }

        public static readonly PropertyData NotebookPagesPanelProperty = RegisterProperty("NotebookPagesPanel", typeof(NotebookPagesPanelViewModel));

        /// <summary>
        /// DisplayPanel.
        /// </summary>
        public DisplayListPanelViewModel DisplayListPanel
        {
            get { return GetValue<DisplayListPanelViewModel>(DisplayListPanelProperty); }
            set { SetValue(DisplayListPanelProperty, value); }
        }

        public static readonly PropertyData DisplayListPanelProperty = RegisterProperty("DisplayListPanel", typeof(DisplayListPanelViewModel));
         
        #endregion //Panels

        #endregion //Bindings

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {

            if (propertyName == "IsAuthoring")
            {                
                SelectedDisplay = MirrorDisplay;
                if ((viewModel as MainWindowViewModel).IsAuthoring)
                {
                   // SelectedDisplay.IsOnProjector = false;
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;
                }
                else
                {
                    WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
                    App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Collapsed;
                }
            }

            if (propertyName == "SideBarVisibility")
            {
                LeftPanel = NotebookPagesPanel;
                LeftPanel.IsVisible = (viewModel as RibbonViewModel).SideBarVisibility;
            }

            if(propertyName == "DisplayPanelVisibility")
            {
                RightPanel = DisplayListPanel;
                RightPanel.IsVisible = (viewModel as RibbonViewModel).DisplayPanelVisibility;
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
            else
            {
                return "";
            }
            foreach(var p in SubmissionPages) {
                UserNames.Remove(p.Submitter.FullName);

            }
            string names = "";
            foreach(string user in UserNames)
            {
                names = names + user + "\n";
            }
            names = names.Substring(0, names.Length - 2);
            return names;
        }

        public void OnlyGroupSubmissionsFilter(object sender, FilterEventArgs e)
        {
            CLPPage page = e.Item as CLPPage;
            if(page != null)
            {
                if(page.SubmissionType == SubmissionType.Group)
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
                if(page.SubmissionType == SubmissionType.Group)
                {
                    e.Accepted = false;
                }
                else
                {
                    e.Accepted = true;
                }
            }
        }

        public ObservableCollection<Tag> getAllTags(ObservableCollection<ICLPPage> pages) 
        {
            ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
            foreach (var page in pages) {
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
               FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
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
                FilteredSubmissions.SortDescriptions.Clear();
                FilteredSubmissions.GroupDescriptions.Add(correctnessDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            }
            else if(Sort == "Starred")
            {
                FilteredSubmissions.GroupDescriptions.Clear();
                FilteredSubmissions.SortDescriptions.Clear();
                FilteredSubmissions.GroupDescriptions.Add(starredDescription);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            }
            else
            {
                FilteredSubmissions.GroupDescriptions.Clear();
                FilteredSubmissions.SortDescriptions.Clear();

                PropertyGroupDescription pgd = new PropertyGroupDescription(null, new PageToTagConverter(Sort));
                FilteredSubmissions.GroupDescriptions.Add(pgd);
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);

            }
        }
        
        #endregion //Methods

    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
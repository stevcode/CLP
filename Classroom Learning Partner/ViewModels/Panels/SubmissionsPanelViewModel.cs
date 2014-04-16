using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(HoverBoxViewModel))]
    public class SubmissionsPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaysPanelViewModel" /> class.
        /// </summary>
        public SubmissionsPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            
            Initialized += SubmissionsPanelViewModel_Initialized;

            #region Tag Stuff

            FilteredSubmissions = new CollectionViewSource();
            FilterTypes = new ObservableCollection<string>();
            FilterTypes.Add("Student Name - Alphabetical");
            FilterTypes.Add("Group Submissions");
            FilterTypes.Add("Submissions By Group Name");
            FilterTypes.Add("Time In - Ascending");
            FilterTypes.Add("Time In - Descending");

            // TODO: Entities
            //ObservableCollection<Tag> tags = getAllTags(Notebook.Pages);

            //foreach(Tag t in tags)
            //{
            //    if(t.TagType != null)
            //    {
            //        FilterTypes.Add(t.TagType.Name);
            //    }
            //}

            // Just hardcode in some tag types for now
            // TODO: Entities
            //FilterTypes.Add(RepresentationCorrectnessTagType.Instance.Name);
            //FilterTypes.Add(ArrayXAxisStrategyTagType.Instance.Name);
            //FilterTypes.Add(ArrayYAxisStrategyTagType.Instance.Name);
            //FilterTypes.Add(ArrayPartialProductsStrategyTagType.Instance.Name);
            //FilterTypes.Add(ArrayDivisionCorrectnessTagType.Instance.Name);
            //FilterTypes.Add(ArrayHorizontalDivisionsTagType.Instance.Name);
            //FilterTypes.Add(ArrayVerticalDivisionsTagType.Instance.Name);
            //FilterTypes.Add(ArrayOrientationTagType.Instance.Name);
            //FilterTypes.Add(StampPartsPerStampTagType.Instance.Name);
            //FilterTypes.Add(StampGroupingTypeTagType.Instance.Name);

            #endregion //Tag Stuff

            ToggleNoSubmissionsCommand = new Command<RoutedEventArgs>(OnToggleNoSubmissionsCommandExecute);
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
        }

        void SubmissionsPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
        }

        public override string Title
        {
            get { return "SubmissionsPanelVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Current, selected submission.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// List of student names who don't have submissions for the CurrentPage.
        /// </summary>
        public ObservableCollection<string> StudentsWithNoSubmissions
        {
            get { return GetValue<ObservableCollection<string>>(StudentsWithNoSubmissionsProperty); }
            set { SetValue(StudentsWithNoSubmissionsProperty, value); }
        }

        public static readonly PropertyData StudentsWithNoSubmissionsProperty = RegisterProperty("StudentsWithNoSubmissions",
                                                                                                 typeof(ObservableCollection<string>),
                                                                                                 () => new ObservableCollection<string>());

        /// <summary>
        /// Whether the panel showing students with no submissions is visible.
        /// </summary>
        public bool IsStudentsWithNoSubmissionsVisible
        {
            get { return GetValue<bool>(IsStudentsWithNoSubmissionsVisibleProperty); }
            set { SetValue(IsStudentsWithNoSubmissionsVisibleProperty, value); }
        }

        public static readonly PropertyData IsStudentsWithNoSubmissionsVisibleProperty = RegisterProperty("IsStudentsWithNoSubmissionsVisible", typeof(bool), false);

        #endregion //Bindings

        #region Properties

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
            }
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource FilteredSubmissions
        {
            get { return GetValue<CollectionViewSource>(FilteredSubmissionsProperty); }
            set { SetValue(FilteredSubmissionsProperty, value); }
        }

        public static readonly PropertyData FilteredSubmissionsProperty = RegisterProperty("FilteredSubmissions", typeof(CollectionViewSource));

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

        public static readonly PropertyData FilterTypesProperty = RegisterProperty("FilterTypes", typeof(ObservableCollection<string>));

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

        public static readonly PropertyData SelectedFilterTypeProperty = RegisterProperty("SelectedFilterType", typeof(string));

        #endregion //Propertes

        #region Commands

        /// <summary>
        /// Toggles the panel that shows the student names who haven't submitted.
        /// </summary>
        public Command<RoutedEventArgs> ToggleNoSubmissionsCommand { get; private set; }

        private void OnToggleNoSubmissionsCommandExecute(RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            if(toggleButton.IsChecked != null &&
               !(bool)toggleButton.IsChecked)
            {
                return;
            }

            StudentsWithNoSubmissions = GetStudentsWithNoSubmissions();
        }

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            if(notebookWorkspaceViewModel.CurrentDisplay == null)
            {
                CurrentPage = page;
                return;
            }

            notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
        }

        #endregion //Commands

        #region Methods

        public ObservableCollection<string> GetStudentsWithNoSubmissions()
        {
            var userNames = new ObservableCollection<string>();

            foreach(var availableUser in App.MainWindowViewModel.AvailableUsers)
            {
                userNames.Add(availableUser.FullName);
            }

            foreach(var p in SubmissionPages.Where(p => userNames.Contains(p.Owner.FullName))) 
            {
                userNames.Remove(p.Owner.FullName);
            }
            return userNames;
        }

        public void OnlyGroupSubmissionsFilter(object sender, FilterEventArgs e)
        {
            var page = e.Item as CLPPage;
            if(page == null)
            {
                return;
            }

            e.Accepted = page.SubmissionType == SubmissionTypes.Group;
        }

        public void OnlyIndividualSubmissionsFilter(object sender, FilterEventArgs e)
        {
            var page = e.Item as CLPPage;
            if(page == null)
            {
                return;
            }

            e.Accepted = page.SubmissionType != SubmissionTypes.Group;
        }

        // TODO: Entities
        //public ObservableCollection<Tag> getAllTags(ObservableCollection<ICLPPage> pages)
        //{
        //    var tags = new ObservableCollection<Tag>();
        //    foreach(var page in pages)
        //    {
        //        foreach(Tag tag in page.PageTags)
        //        {
        //            if(!tags.Contains(tag))
        //            {
        //                tags.Add(tag);
        //            }
        //        }
        //    }
        //    return tags;
        //}

        public void FilterSubmissions(string Sort)
        {
            IsStudentsWithNoSubmissionsVisible = false;
            FilteredSubmissions = new CollectionViewSource
                                  {
                                      Source = SubmissionPages
                                  };
            //StudentsWithNoSubmissions = getStudentsWithNoSubmissions();

            var submitterNameDescription = new PropertyGroupDescription("Owner.FullName");
            //PropertyGroupDescription groupNameDescription = new PropertyGroupDescription("GroupName", new GroupLabelConverter());
            //PropertyGroupDescription timeDescription = new PropertyGroupDescription("SubmissionTime");
            //PropertyGroupDescription isGroupDescription = new PropertyGroupDescription("IsGroupSubmission", new BooleantoGroupConverter());
            //PropertyGroupDescription correctnessDescription = new PropertyGroupDescription(null, new PagetToCorrectnessTagConverter());
            //PropertyGroupDescription starredDescription = new PropertyGroupDescription(null, new PagetToStarredTagConverter());

            var submitterNameSort = new SortDescription("Owner.FullName", ListSortDirection.Ascending);
            //SortDescription groupNameSort = new SortDescription("GroupName", ListSortDirection.Ascending);
            var timeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);
            //SortDescription timeAscendingSort = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
            //SortDescription isGroupSubmissionSort = new SortDescription("IsGroupSubmission", ListSortDirection.Ascending);

            if(Sort == "Student Name - Alphabetical")
            {
                FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
                FilteredSubmissions.SortDescriptions.Add(submitterNameSort);
                FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
            }

            //else if(Sort == "Group Submissions")
            //{
            //    FilteredSubmissions.Filter += new FilterEventHandler(OnlyGroupSubmissionsFilter);

            //    FilteredSubmissions.GroupDescriptions.Add(groupNameDescription);
            //    FilteredSubmissions.SortDescriptions.Add(groupNameSort);
            //    FilteredSubmissions.GroupDescriptions.Add(timeDescription);

            //    FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            //    FilteredSubmissions.SortDescriptions.Add(submitterNameSort);

            //}
            //else if(Sort == "Submissions By Group Name")
            //{
            //    FilteredSubmissions.Filter += new FilterEventHandler(OnlyIndividualSubmissionsFilter);

            //    FilteredSubmissions.GroupDescriptions.Add(groupNameDescription);
            //    FilteredSubmissions.SortDescriptions.Add(groupNameSort);

            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            //    FilteredSubmissions.SortDescriptions.Add(submitterNameSort);
            //}

            //else if(Sort == "Time In - Ascending")
            //{

            //    FilteredSubmissions.GroupDescriptions.Add(timeDescription);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);

            //    FilteredSubmissions.SortDescriptions.Add(timeAscendingSort);
            //}
            //else if(Sort == "Time In - Descending")
            //{
            //    FilteredSubmissions.GroupDescriptions.Add(timeDescription);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);

            //    FilteredSubmissions.SortDescriptions.Add(timeDescendingSort);
            //}
            //else if(Sort == "Correctness")
            //{
            //    FilteredSubmissions.GroupDescriptions.Clear();
            //    FilteredSubmissions.SortDescriptions.Clear();
            //    FilteredSubmissions.GroupDescriptions.Add(correctnessDescription);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            //}
            //else if(Sort == "Starred")
            //{
            //    FilteredSubmissions.GroupDescriptions.Clear();
            //    FilteredSubmissions.SortDescriptions.Clear();
            //    FilteredSubmissions.GroupDescriptions.Add(starredDescription);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);
            //}
            //else
            //{
            //    FilteredSubmissions.GroupDescriptions.Clear();
            //    FilteredSubmissions.SortDescriptions.Clear();

            //    PropertyGroupDescription pgd = new PropertyGroupDescription(null, new PageToTagConverter(Sort));
            //    FilteredSubmissions.GroupDescriptions.Add(pgd);
            //    FilteredSubmissions.GroupDescriptions.Add(submitterNameDescription);

            //}
        }

        #endregion //Methods
    }
}
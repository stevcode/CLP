using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(HoverBoxViewModel))]
    public class SubmissionsPanelViewModel : ViewModelBase, IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayListPanelViewModel"/> class.
        /// </summary>
        public SubmissionsPanelViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;

            StudentsWithNoSubmissions = getStudentsWithNoSubmissions();

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
            FilterTypes.Add(ArrayPartialProductsStrategyTagType.Instance.Name);
            FilterTypes.Add(ArrayDivisionCorrectnessTagType.Instance.Name);
            FilterTypes.Add(ArrayHorizontalDivisionsTagType.Instance.Name);
            FilterTypes.Add(ArrayVerticalDivisionsTagType.Instance.Name);
            FilterTypes.Add(ArrayOrientationTagType.Instance.Name);
            FilterTypes.Add(StampPartsPerStampTagType.Instance.Name);
            FilterTypes.Add(StampGroupingTypeTagType.Instance.Name);

            #endregion //Tag Stuff

            SetCurrentPageCommand = new Command<ICLPPage>(OnSetCurrentPageCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "SubmissionsPanelVM"; } }

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Current, selected submission.
        /// </summary>
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage));

        #endregion //Bindings

        #region Properties

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
            set {  SetValue(FilterTypesProperty, value); }
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

        #region IPanel Members

        public string PanelName
        {
            get
            {
                return "SubmissionsPanel";
            }
        }

        /// <summary>
        /// Whether the Panel is pinned to the same Z-Index as the Workspace.
        /// </summary>
        public bool IsPinned
        {
            get { return GetValue<bool>(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        public static readonly PropertyData IsPinnedProperty = RegisterProperty("IsPinned", typeof(bool), true);

        /// <summary>
        /// Visibility of Panel, True for Visible, False for Collapsed.
        /// </summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), false);

        /// <summary>
        /// Can the Panel be resized.
        /// </summary>
        public bool IsResizable
        {
            get { return GetValue<bool>(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), false);

        /// <summary>
        /// Initial Width of the Panel, before any resizing.
        /// </summary>
        public double InitialWidth
        {
            get { return 250; }
        }

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Right);

        /// <summary>
        /// A Linked IPanel if more than one IPanel is to be used in the same Location.
        /// </summary>
        public IPanel LinkedPanel
        {
            get { return GetValue<IPanel>(LinkedPanelProperty); }
            set { SetValue(LinkedPanelProperty, value); }
        }

        public static readonly PropertyData LinkedPanelProperty = RegisterProperty("LinkedPanel", typeof(IPanel));

        #endregion

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<ICLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(ICLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.SelectedDisplay.AddPageToDisplay(page);
            }
        }

        #endregion //Commands

        #region Methods

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
            foreach(var p in SubmissionPages)
            {
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
                else
                {
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
            foreach(var page in pages)
            {
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
            FilteredSubmissions = new CollectionViewSource
                                  {
                                      Source = SubmissionPages
                                  };
            //StudentsWithNoSubmissions = getStudentsWithNoSubmissions();

            var submitterNameDescription = new PropertyGroupDescription("Submitter.FullName");
            //PropertyGroupDescription groupNameDescription = new PropertyGroupDescription("GroupName", new GroupLabelConverter());
            //PropertyGroupDescription timeDescription = new PropertyGroupDescription("SubmissionTime");
            //PropertyGroupDescription isGroupDescription = new PropertyGroupDescription("IsGroupSubmission", new BooleantoGroupConverter());
            //PropertyGroupDescription correctnessDescription = new PropertyGroupDescription(null, new PagetToCorrectnessTagConverter());
            //PropertyGroupDescription starredDescription = new PropertyGroupDescription(null, new PagetToStarredTagConverter());

            var submitterNameSort = new SortDescription("Submitter.FullName", ListSortDirection.Ascending);
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

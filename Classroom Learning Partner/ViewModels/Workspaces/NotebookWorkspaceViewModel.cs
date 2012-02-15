using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;
using System.Windows.Media;
using System;
using Classroom_Learning_Partner.Model;
using System.Windows;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(MainWindowViewModel))]
    [InterestedIn(typeof(SideBarViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel()
            : base()
        {
            Console.WriteLine(Title + " created");
            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            //NotebookPages = new ObservableCollection<CLPPageViewModel>();
            //foreach (var page in Notebook.Pages)
            //{
            //    NotebookPages.Add(new CLPPageViewModel(page));
            //}

            //SideBar = new SideBarViewModel(NotebookPages);

            //LinkedDisplay = new LinkedDisplayViewModel(NotebookPages[0]);

            SideBar = new SideBarViewModel(App.MainWindowViewModel.OpenNotebooks[App.MainWindowViewModel.CurrentNotebookIndex]);
            SideBar.SelectedNotebookPage = SideBar.Pages[0];
            SelectedDisplay = new LinkedDisplayViewModel(SideBar.CurrentPage);

            LinkedDisplay = SelectedDisplay as LinkedDisplayViewModel;
            SelectedDisplay.IsActive = true;

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                SelectedDisplay.IsOnProjector = true;
            }
            else
            {
                SelectedDisplay.IsOnProjector = false;
            }
        }

        protected override void Close()
        {
            Console.WriteLine(Title + " closed");
            base.Close();
        }

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //public ObservableCollection<CLPPageViewModel> NotebookPages
        //{
        //    get { return GetValue<ObservableCollection<CLPPageViewModel>>(NotebookPagesProperty); }
        //    set { SetValue(NotebookPagesProperty, value); }
        //}

        ///// <summary>
        ///// Register the NotebookPages property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData NotebookPagesProperty = RegisterProperty("NotebookPages", typeof(ObservableCollection<CLPPageViewModel>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public SideBarViewModel SideBar
        {
            get { return GetValue<SideBarViewModel>(SideBarProperty); }
            private set { SetValue(SideBarProperty, value); }
        }

        /// <summary>
        /// Register the SideBar property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SideBarProperty = RegisterProperty("SideBar", typeof(SideBarViewModel));

        /// <summary>
        /// Gets or sets the property value.
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
        /// Gets or sets the property value.
        /// </summary>
        public IDisplayViewModel SelectedDisplay
        {
            get { return GetValue<IDisplayViewModel>(SelectedDisplayProperty); }
            set { SetValue(SelectedDisplayProperty, value); }
        }

        /// <summary>
        /// Register the SelectedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(IDisplayViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Register the WorkspaceBackgroundColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "IsAuthoring")
            {
                SelectedDisplay = LinkedDisplay;
                if ((viewModel as MainWindowViewModel).IsAuthoring)
                {
                    SelectedDisplay.IsOnProjector = false;
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    App.MainWindowViewModel.AuthoringTabVisibility = Visibility.Visible;
                }
                else
                {
                    WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
                    App.MainWindowViewModel.AuthoringTabVisibility = Visibility.Collapsed;
                    if (App.CurrentUserMode == App.UserMode.Instructor)
                    {
                        SelectedDisplay.IsOnProjector = true;
                    }
                }
            }

            if (propertyName == "CurrentPage")
            {
                if (SelectedDisplay != null)
                {
                	SelectedDisplay.AddPageToDisplay((viewModel as SideBarViewModel).CurrentPage);
                }
            }

            //if (propertyName == "CurrentNotebookIndex")
            //{
            //    int index = (viewModel as MainWindowViewModel).CurrentNotebookIndex;
            //    Notebook = App.MainWindowViewModel.OpenNotebooks[index];
            //    //NotebookPages.Clear();
            //    //foreach (var page in Notebook.Pages)
            //    //{
            //    //    NotebookPages.Add(new CLPPageViewModel(page));
            //    //}
            //}

            base.OnViewModelPropertyChanged(viewModel, propertyName);
            
        }
    }
}
